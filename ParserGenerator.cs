using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scriban;
using Scriban.Runtime;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace CommandLineArgsGenerator
{
    [Generator]
    public class ParserGenerator : ISourceGenerator
    {
        private static SymbolDisplayFormat typeFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
        private static readonly Template template;
        static ParserGenerator()
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(asm.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(".template")));
            using var reader = new System.IO.StreamReader(stream);
            template = Template.Parse(reader.ReadToEnd());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = context.SyntaxContextReceiver as ParserSyntaxReceiver;
            if (receiver?.Class != null)
            {
                var methods = GetMethods(receiver.Class);
                List<CommandInfo> commandInfos = new List<CommandInfo>();
                foreach (var method in methods)
                {
                    var documentation = GetXmlDocumentation(method);
                    (ParameterInfo[] parameters, OptionInfo[] options) = GetParamsAndOptions(method, documentation, context.Compilation.GetSemanticModel(receiver.Class.SyntaxTree));

                    var fullname = $"{ParserSyntaxReceiver.GetFullName(receiver.Class)}.{method.Identifier}";
                    commandInfos.Add(new CommandInfo()
                    {
                        FullName = fullname,
                        HelpText = documentation.Descendants(XName.Get("summary")).FirstOrDefault()?.Value.Trim() ?? string.Empty,
                        Name = TransformName(method.Identifier.ToString()),
                        Parameters = parameters,
                        Options = options
                    }); ;
                } 

                var sc = new ScriptObject();  
                
                sc.Import("HasMethod", (Func<ParameterInfo, string, int, bool>)((param,name,args) => param.HasMethod(name, args)));
                sc.Import("HasCtor", (Func<ParameterInfo, IEnumerable<object>, bool>)((param, args) => param.HasCtor(args.Cast<string>().ToArray())));
                sc.Import("GroupByVal", (Func<Dictionary<string,string>, Dictionary<string,List<string>>>)(d => d.GroupBy(x => x.Value).ToDictionary(t => t.Key, t => t.Select(r => r.Key).ToList())));
                 
                var ctx = new TemplateContext();
                ctx.PushGlobal(sc);
                sc.Import(
                    new 
                    { 
                        Namespace = (receiver.Class.Parent as NamespaceDeclarationSyntax).Name.ToString(), 
                        Commands = commandInfos, 
                        Converters = receiver.Converters 
                    }, x => true , x => x.Name);
                ctx.MemberRenamer = x => x.Name;
                
                System.IO.File.WriteAllText("D:\\ep.cs", 
                    template.Render(ctx));  
                context.AddSource("EntryPoint.cs", template.Render(ctx));
 
            }
        }

        private IEnumerable<MethodDeclarationSyntax> GetMethods(ClassDeclarationSyntax @class)
        {
            return @class.Members
                .Where(x => x is MethodDeclarationSyntax)
                .Cast<MethodDeclarationSyntax>()
                .Where(x => x.AttributeLists
                        .All(attr => attr.Attributes
                                    .FirstOrDefault(a => a.Name.ToString() == "Ignore"
                                            || a.Name.ToString() == "IgnoreAttribute") == null))
                .Where(m => m.Modifiers.Any(SyntaxKind.PublicKeyword)
                        && m.Modifiers.Any(SyntaxKind.StaticKeyword))

                ;
        }




        private XDocument GetXmlDocumentation(SyntaxNode node)
        {
            var xml = node.GetLeadingTrivia();
            var sb = new StringBuilder();
            sb.AppendLine("<doc>");
            foreach (var item in xml.Where(x => x.IsKind(SyntaxKind.SingleLineCommentTrivia)))
            {
                sb.AppendLine(item.ToString().TrimStart('/'));
            }
            sb.AppendLine("</doc>");
            var documentation = XDocument.Parse(sb.ToString());
            return documentation;
        }





        private ParameterInfo GetParamOrOption(ParameterSyntax param, SemanticModel semanticModel, XDocument documentation)
        {
            string name = param.Identifier.ValueText;

            var typeInfo = semanticModel.GetTypeInfo(param.Type);
            var p = documentation.Descendants("param")
                .FirstOrDefault(p => p.Attribute("name")?.Value == name);

            string help = p
                ?.Value ?? "";

            var type = (typeInfo.Type);
            string displayTypeName = typeInfo.Type.ToDisplayString(typeFormat);
            if (param.Default == null && type?.TypeKind != TypeKind.Array)
            {
                return new ParameterInfo
                {
                    RawName = name,
                    Name = TransformName(name),
                    Type = type as INamedTypeSymbol,
                    HelpText = help,
                    DisplayTypeName = displayTypeName,
                };
            }
            else if (type?.TypeKind == TypeKind.Array)
            {
                var t = (type as IArrayTypeSymbol).ElementType as INamedTypeSymbol;
                return new OptionInfo
                {
                    RawName = name,
                    Name = TransformName(name),
                    Type = t,
                    HelpText = help,
                    DisplayTypeName = t.ToDisplayString(typeFormat),
                    IsArray = true,
                    Default = param.Default?.ToString().TrimStart('=') ?? "null"
                };
            }
            else
            {
                return new OptionInfo
                {
                    RawName = name,
                    Name = TransformName(name),
                    Type = type as INamedTypeSymbol,
                    HelpText = help,
                    DisplayTypeName = displayTypeName,
                    Default = param.Default?.ToString().TrimStart('=') ?? "null"
                };
            }
        }
        private (ParameterInfo[] parameters, OptionInfo[] options) GetParamsAndOptions(
                MethodDeclarationSyntax method,
                XDocument documentation,
                SemanticModel semanticModel)
        {

            var pars = method.ParameterList.Parameters
                    .Select(param => GetParamOrOption(param, semanticModel, documentation)).ToArray();

            var parameters = pars.Where(param => param is not OptionInfo).ToArray();

            var options = pars.Where(param => param is OptionInfo).Cast<OptionInfo>().ToArray();

            return (parameters, options);

        }
        private string TransformName(string name)
        {
            var sb = new StringBuilder();
            sb.Append(char.ToLower(name[0]));
            foreach (var item in name.AsSpan(1))
            {
                if (char.IsUpper(item))
                {
                    sb.Append('-');
                    sb.Append(char.ToLower(item));
                }
                else
                {
                    sb.Append(item);
                }
            }
            return sb.ToString();
        }

        public void Initialize(GeneratorInitializationContext context)
        { 
            context.RegisterForSyntaxNotifications(() => new ParserSyntaxReceiver());
        }
    }
}
