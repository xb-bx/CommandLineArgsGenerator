﻿using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;

namespace CommandLineArgsGenerator
{
    [Generator]
    public class ParserGenerator : ISourceGenerator
    {
        private static SymbolDisplayFormat typeFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
        public void Execute(GeneratorExecutionContext context)
        { 
            var receiver = context.SyntaxContextReceiver as ParserSyntaxReceiver; 
            if (receiver?.Root?.Class != null && receiver.Namespace != null)
            {
                var semanticModel = context.Compilation.GetSemanticModel(receiver.Root.Class.SyntaxTree);
                var cmds = GetCommands(receiver.Root.Class, semanticModel, out ICommandInfo defaultCommand);
                receiver.Root.Children = cmds;
                receiver.Root.Default = defaultCommand;
                var t = new
                    {
                        Namespace = receiver.Namespace.Trim(),
                        Root = receiver.Root,
                        Converters = receiver.Converters.GroupBy(x => x.Value).ToDictionary(t => t.Key, t => t.Select(r => r.Key).ToList())
                    }; 
                context.AddSource("EntryPoint.cs",  Template.RenderAsync(t).Result);

            }

        }

        private List<ICommandInfo> GetCommands(ClassDeclarationSyntax @class, SemanticModel semanticModel, out ICommandInfo defaultCommand, string prevName = null)
        {
            defaultCommand = null;
            var cmds = new List<ICommandInfo>();
            var methods = GetMethods(@class);
            foreach (var method in methods)
            {
                var doc = GetXmlDocumentation(method);
                var rawName = method.Identifier.ToString().Trim();
                var paramAndOpts = GetParamsAndOptions(method, doc, semanticModel);
                var name = TransformName(rawName); 
                var cmd = new CommandInfo
                    {
                        Name = name,
                        RawName = rawName,
                        NameInSourceCode = ParserSyntaxReceiver.GetFullName(method),
                        FullName = prevName is not null ? $"{prevName} {name}" : name,
                        Parameters = paramAndOpts.parameters,
                        Options = paramAndOpts.options,
                        HelpText = doc.Descendants("summary").FirstOrDefault()?.Value.Trim(),
                    };
                if(method.AttributeLists.Any(x => x.Attributes.Any(attr => attr.Name.ToString() is "Default")))
                {
                    defaultCommand = cmd;
                }
                else
                {
                    cmds.Add(cmd);    
                }
                
            }
            var classes = GetClasses(@class);
            foreach (var cl in classes)
            {
                var doc = GetXmlDocumentation(cl);
                var rawName = cl.Identifier.ToString().Trim();
                var name = TransformName(rawName);
                var cmd = new RootCommand
                {
                    Name = name,
                    FullName = $"{prevName} {name}",
                    HelpText = doc.Descendants("summary").FirstOrDefault()?.Value.Trim(),
                    Children = GetCommands(cl, semanticModel, out ICommandInfo defCmd, name),
                    Class = cl,
                    RawName = rawName,
                    Default = defCmd
                };
                if(cl.AttributeLists.Any(x => x.Attributes.Any(attr => attr.Name.ToString() is "Default")))
                {
                    defaultCommand = cmd;
                }
                else
                {
                    cmds.Add(cmd);
                }
            }
            return cmds;
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
        private IEnumerable<ClassDeclarationSyntax> GetClasses(ClassDeclarationSyntax @class) 
        {
            return @class.Members
                .Where(x => x is ClassDeclarationSyntax && x.Modifiers.Any(SyntaxKind.PublicKeyword))
                .Cast<ClassDeclarationSyntax>();
                
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
                ?.Value.Trim() ?? "";

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
                    DisplayTypeName = type.ToDisplayString(typeFormat),
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
                else if(item is not '@')
                {
                    sb.Append(item);
                }
            }
            return sb.ToString();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            if (!Debugger.IsAttached) 
            {
               // Debugger.Launch();
            }
            context.RegisterForSyntaxNotifications(() => new ParserSyntaxReceiver()); 
        }
    }
}