using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scriban;
using Scriban.Runtime;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using Scriban.Parsing;
using CommandLineArgsGenerator.Styles;

namespace CommandLineArgsGenerator
{
    [Generator]
    public class ParserGenerator : ISourceGenerator
    {
        public static readonly DiagnosticDescriptor GenerationError =
            new DiagnosticDescriptor
                (
                "CLAG001",
                "Error during generation",
                "{0}",
                "CommandLineArgsGenerator",
                DiagnosticSeverity.Error,
                true
                );
        private static SymbolDisplayFormat typeFormat = new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
        private static readonly Template parserTemplate, helpTextsTemplate, enumParserTemplate, completionTemplate;
		private string defaultLanguage = "";
        private NamingStyle paramStyle = NamingStyle.AllVariants;
        private NamingStyle enumStyle = NamingStyle.AllVariants;
        static ParserGenerator()
        {
            var asm = Assembly.GetExecutingAssembly();
			var manifestNames = asm.GetManifestResourceNames();
			foreach(string name in manifestNames)
			{ 
				using var stream = asm.GetManifestResourceStream(name);
				using var reader = new System.IO.StreamReader(stream);
                switch(name)
                {
                    case "CommandLineArgsGenerator.templates.EnumParser.template": 
			    		enumParserTemplate = Template.Parse(reader.ReadToEnd());
                        break;
                    case "CommandLineArgsGenerator.templates.HelpTexts.template":
                        helpTextsTemplate = Template.Parse(reader.ReadToEnd());
                        break;
                    case "CommandLineArgsGenerator.templates.Parser.template": 
					    parserTemplate = Template.Parse(reader.ReadToEnd());
                        break;
                    case "CommandLineArgsGenerator.templates.Completion.template": 
					    completionTemplate = Template.Parse(reader.ReadToEnd());
                        break;
                }
			}
		}
		
        public void Execute(GeneratorExecutionContext context)
        {        
            var receiver = context.SyntaxContextReceiver as ParserSyntaxReceiver; 
            if (receiver?.Root?.Class is not null && receiver.Namespace is not null)
            {
				defaultLanguage = context.GetMSBuildProperty("DefaultLanguage");
                defaultLanguage = string.IsNullOrWhiteSpace(defaultLanguage) ? "en" : defaultLanguage;
				var semanticModel = context.Compilation.GetSemanticModel(receiver.Root.Class.SyntaxTree);
                paramStyle = receiver.Root.ParamStyle;
                enumStyle = receiver.Root.EnumStyle;
                var cmds = GetCommands(receiver.Root.Class, semanticModel, out CommandInfoBase? defaultCommand, paramStyle);
                var rootDoc = GetXmlDocumentation(receiver.Root.Class);

				var rh = rootDoc.Descendants("summary").FirstOrDefault();
				if(rh is not null)
				{
					receiver.Root.HelpText = HelpText.FromXElement(rh, defaultLanguage);
				}
                receiver.Root.Children = cmds;
                receiver.Root.Default = defaultCommand;
				
                var enums = 
                    GetEnumsInfo(defaultCommand is null ? cmds : cmds.Append(defaultCommand))
                    .Select(x => (x.type.GetMembers().Select(y => y.Name).ToArray(), x.typeName,x.displayTypeName))
                    .ToArray(); 
                
                bool.TryParse(context.GetMSBuildProperty("GenerateCompletion"), out bool genComp);
                bool.TryParse(context.GetMSBuildProperty("GenerateSuggestions"), out bool genSugg);
               
                Queue<string> errors = new();
                var ctx = CreateContext(CreateParserTemplateModel(receiver, context), errors);
                string entryPoint = parserTemplate.Render(ctx);
				string completion = genComp ? completionTemplate.Render(ctx) : "";
                ctx = CreateContext(CreateHelpTextsModel(receiver), errors);
				string helpTexts = helpTextsTemplate.Render(ctx);
                ctx = CreateContext(CreateEnumsModel(receiver.Namespace, enums), errors);
                string enumParser = enumParserTemplate.Render(ctx);
                
                var logPath = context.GetMSBuildProperty("LogGeneratedParser"); 
                if(string.IsNullOrWhiteSpace(logPath) is not true)
				{
					File.WriteAllText(Path.Combine(logPath, "EntryPoint.cs"), entryPoint);
					File.WriteAllText(Path.Combine(logPath, "HelpTexts.cs"), helpTexts); 
                    File.WriteAllText(Path.Combine(logPath, "EnumParser.cs"), enumParser);
                    if(genComp)
                        File.WriteAllText(Path.Combine(logPath, "Completer.cs"), completion);
                }
                foreach(var item in errors)
                    context.ReportDiagnostic(Diagnostic.Create(GenerationError, null, item));
                context.AddSource("EntryPoint.cs", entryPoint);
                context.AddSource("HelpTexts.cs", helpTexts);
                context.AddSource("EnumParser.cs", enumParser);               
                if(genComp)
                    context.AddSource("Completer.cs", completion);                
			}
        }
        public IEnumerable<(INamedTypeSymbol type, string typeName, string displayTypeName)> GetEnumsInfo(IEnumerable<CommandInfoBase> commands)
        {
            var enums = 
            commands
                .OfType<CommandInfo>()
                .SelectMany
                (
                    x => x.Parameters
                            .Where(param => param.IsEnum)
                            .Concat(
                                x.Options.Where(option => option.IsEnum)
                            )
                )
                .Select(p => (p.Type, p.UnderscoredTypeName, p.DisplayTypeName));
            var ens =
            commands
                .OfType<RootCommand>()
                .SelectMany(
                    cmd => 
                        cmd.Default is null ? 
                        GetEnumsInfo(cmd.Children) : 
                        GetEnumsInfo(cmd.Children.Append(cmd.Default))
                    );
            enums = enums.Concat(ens);
            return enums;
                
        }
		private (string[] optionPrefixes, string[] aliasPrefixes) GetPrefixArrays(PrefixStyle argPrefix)
		{
            var optionPrefixes = new List<string>();
            var aliasPrefixes = new List<string>();
            if (argPrefix.HasFlag(PrefixStyle.DoubleHyphen)) { optionPrefixes.Add("--"); }
            if (argPrefix.HasFlag(PrefixStyle.Hyphen)) { optionPrefixes.Add("-"); aliasPrefixes.Add("-"); }
            if (argPrefix.HasFlag(PrefixStyle.Slash)) { optionPrefixes.Add("/"); aliasPrefixes.Add("/"); }
            if (aliasPrefixes.Count == 0) aliasPrefixes.Add("-"); // fallback for aliases
            return (optionPrefixes.ToArray(), aliasPrefixes.ToArray());
		}
		private object CreateParserTemplateModel(ParserSyntaxReceiver receiver, GeneratorExecutionContext context)
		{
            bool.TryParse(context.GetMSBuildProperty("GenerateCompletion"), out bool genComp);
            bool.TryParse(context.GetMSBuildProperty("GenerateSuggestions"), out bool genSugg);
            var root = receiver.Root!;
            var (optionPrefixes, aliasPrefixes) = GetPrefixArrays(root.ArgPrefix);
			return new {
				Namespace = receiver.Namespace,
				Root = root,
				Converters = receiver.Converters.GroupBy(x => x.Value).ToDictionary(t => t.Key, t => t.Select(r => r.Key).ToList()),
		        GenerateCompletion = genComp,
                GenerateSuggestions = genSugg,
                ParamCaseInsensitive = root.ParamStyle.HasFlag(NamingStyle.CaseInsensitive),
                SeparatorColon = root.ValueSeparator.HasFlag(SeparatorStyle.Colon),
                SeparatorEquals = root.ValueSeparator.HasFlag(SeparatorStyle.Equals),
                OptionPrefixes = optionPrefixes,
                AliasPrefixes = aliasPrefixes,
                SkipCommandParsing = root.SkipCommandParsing && root.Children.Count == 0 && root.Default != null,
                HelpCommand = root.HelpCommand,
                MandatoryNamed = root.MandatoryStyle == MandatoryStyle.Named || root.MandatoryStyle == MandatoryStyle.Mixed,
                MandatoryPositional = root.MandatoryStyle == MandatoryStyle.Positional || root.MandatoryStyle == MandatoryStyle.Mixed,
            };
		}
		private object CreateHelpTextsModel(ParserSyntaxReceiver receiver)
		{
            var (optionPrefixes, aliasPrefixes) = GetPrefixArrays(receiver.Root!.ArgPrefix);
			return new {
				Namespace = receiver.Namespace,
				Root = receiver.Root,
				DefaultLanguage = defaultLanguage,
				OptionPrefixes = optionPrefixes,
				AliasPrefixes = aliasPrefixes,
				HelpCommand = receiver.Root.HelpCommand,
				MandatoryNamed = receiver.Root!.MandatoryStyle == MandatoryStyle.Named || receiver.Root!.MandatoryStyle == MandatoryStyle.Mixed,
			};
		}
		public object CreateEnumsModel(string @namespace,(string[] values, string typeName, string displayTypeName)[] enums)
        {
            return new {
                Namespace = @namespace,
                EnumsInfo = enums,
                EnumCaseInsensitive = enumStyle.HasFlag(NamingStyle.CaseInsensitive),
                EnumStyleFlags = (int)enumStyle,
            };
        }
        private TemplateContext CreateContext(object model, Queue<string> errors)
		{
			var sc = new ScriptObject();  
			sc.Import("HasMethod", (Func<ParameterInfo, string, int, bool>)((param,name,args) => param.HasMethod(name, args)));
			sc.Import("HasCtor", (Func<ParameterInfo, IEnumerable<object>, bool>)((param, args) => param.HasCtor(args.Cast<string>().ToArray())));
			sc.Import("JoinParamsAndOptions", (Func<CommandInfo, string>)((cmd) => string.Join(", ", cmd.Parameters.Concat(cmd.Options).Select(p => p.RawName))));
			sc.Import("Join", ((Func<IEnumerable<object>, string, string>)((e, c) => string.Join(c, e))));
            var capturedEnumStyle = enumStyle;
            sc.Import("GetEnumMembers", ((Func<INamedTypeSymbol, IEnumerable<string>>)(x => x.GetMembers().Where(m => m.Name != ".ctor").Select(m => GetNameVariants(m.Name, capturedEnumStyle)[0]))));
            sc.Import("GetNameVariants", (Func<string, string[]>)(name => GetNameVariants(name, capturedEnumStyle)));
            sc.Import("Error", (Action<string>)(error => errors.Enqueue(error)));
			var ctx = new TemplateContext();
			ctx.PushGlobal(sc);
			sc.Import(model, x => true , x => x.Name);
			ctx.MemberRenamer = x => x.Name;
			return ctx;
		}
        private void SetNameVariants(CommandInfoBase cmd, NamingStyle style, string[]? parentFullNameVariants)
        {
            cmd.NameVariants = GetNameVariants(cmd.RawName, style);
            cmd.FullNameVariants = parentFullNameVariants != null
                ? parentFullNameVariants.SelectMany(p => cmd.NameVariants.Select(n => p + " " + n)).ToArray()
                : cmd.NameVariants;
            cmd.UnderscoredName = cmd.FullNameVariants[0].Replace(" ", "_").Replace('-', '_').Replace('.', '_');
        }
        private List<CommandInfoBase> GetCommands(ClassDeclarationSyntax @class, SemanticModel semanticModel, out CommandInfoBase? defaultCommand, NamingStyle style, string[]? parentFullNameVariants = null)
        {
            defaultCommand = null;
            var cmds = new List<CommandInfoBase>();
            var methods = GetMethods(@class);
            foreach (var method in methods)
            {

                var doc = GetXmlDocumentation(method);
                var rawName = method.Identifier.ToString().Trim();
                var paramAndOpts = GetParamsAndOptions(method, doc, semanticModel);
				var h = doc.Descendants("summary").FirstOrDefault();
				HelpText? help = null;
				if(h is not null)
				{
					help = HelpText.FromXElement(h, defaultLanguage);
				}
                var cmd = new CommandInfo
                    {
                        RawName = rawName,
                        NameInSourceCode = ParserSyntaxReceiver.GetFullName(method),
                        Parameters = paramAndOpts.parameters,
                        Options = paramAndOpts.options,
                        HelpText = help,
                        IsTask = semanticModel.GetTypeInfo(method.ReturnType).Type?.Name == "Task",
                    };
                SetNameVariants(cmd, style, parentFullNameVariants);
                foreach (var p in paramAndOpts.parameters)
                    p.NameVariants = GetNameVariants(p.RawName, style);
                foreach (var o in paramAndOpts.options)
                    o.NameVariants = GetNameVariants(o.RawName, style);
                bool hasDefaultAttr = method.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(a => a.Name.ToString() == "DefaultCommand" || a.Name.ToString() == "DefaultCommandAttribute");
                if(defaultCommand is null && (hasDefaultAttr || doc.Descendants("summary").FirstOrDefault()?.Attribute("default")?.Value == "true"))
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
                var h = doc.Descendants("summary").FirstOrDefault();
				HelpText? help = null;
				if(h is not null)
				{
					help = HelpText.FromXElement(h, defaultLanguage);
				}
				var cmd = new RootCommand
                {
                    HelpText = help,
                    Class = cl,
                    RawName = rawName,
                };
                SetNameVariants(cmd, style, parentFullNameVariants);
                cmd.Children = GetCommands(cl, semanticModel, out CommandInfoBase? defCmd, style, cmd.FullNameVariants);
                cmd.Default = defCmd;
                bool hasDefaultAttr = cl.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(a => a.Name.ToString() == "DefaultCommand" || a.Name.ToString() == "DefaultCommandAttribute");
                if(defaultCommand is null && (hasDefaultAttr || doc.Descendants("summary").FirstOrDefault()?.Attribute("default")?.Value == "true"))
                {
                    defaultCommand = cmd;
                }
                else
                {
                    cmds.Add(cmd);
                }
            }
            if (defaultCommand is null && cmds.Count == 1 && parentFullNameVariants == null)
            {
                defaultCommand = cmds[0];
                cmds.Clear();
            }
            return cmds;
        }


        private IEnumerable<MethodDeclarationSyntax> GetMethods(ClassDeclarationSyntax @class)
        {
            return @class.Members
                .OfType<MethodDeclarationSyntax>()
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

            var typeInfo = semanticModel.GetTypeInfo(param.Type!);
            var p = documentation.Descendants("param")
                .FirstOrDefault(p => p.Attribute("name")?.Value == name);
			HelpText? help = null;
			if(p is not null)
			{
				help = HelpText.FromXElement(p,defaultLanguage);
			}

            bool isNullable = false;
            if(param.Type is NullableTypeSyntax nts)
            {
                typeInfo = semanticModel.GetTypeInfo(nts.ElementType); 
                isNullable = true;
            } 
            var type = (typeInfo.Type);
            string? displayTypeName = type?.ToDisplayString(typeFormat); 
            if (param.Default == null && type.TypeKind != TypeKind.Array && !isNullable)
            {
                return new ParameterInfo
                {
                    RawName = name,
                    Type = type as INamedTypeSymbol,
                    HelpText = help,
                    DisplayTypeName = displayTypeName,
                    IsNullable = isNullable,
                    Alias = p?.Attribute("alias")?.Value,
                };
            }
            else if (type.TypeKind == TypeKind.Array)
            {
                var t = (type as IArrayTypeSymbol)!.ElementType as INamedTypeSymbol;
                return new OptionInfo
                {
                    RawName = name,
                    Type = t,
                    HelpText = help,
                    DisplayTypeName = t?.ToDisplayString(typeFormat) ?? "FUCK?",
                    IsArray = true,
                    Alias = p?.Attribute("alias")?.Value,
                    Default = param.Default?.ToString().TrimStart('=').TrimStart() ?? "null"
                };
            }
            else
            {
                return new OptionInfo
                {
                    RawName = name,
                    Type = type as INamedTypeSymbol,
                    HelpText = help,
                    DisplayTypeName = displayTypeName ?? "FUCK?",
                    Alias = p?.Attribute("alias")?.Value,
                    Default = param.Default?.ToString().TrimStart('=').TrimStart(),
                    IsNullable = isNullable,
                    
                };
            }
        }
        private (ParameterInfo[] parameters, OptionInfo[] options) GetParamsAndOptions(
                MethodDeclarationSyntax method,
                XDocument documentation,
                SemanticModel semanticModel)
        {

            var pars = method.ParameterList.Parameters
                    .Where(param => param.AttributeLists.All(attrs => !attrs.Attributes.Any(attr => attr.Name.ToString() == "Ignore")))
                    .Select(param => GetParamOrOption(param, semanticModel, documentation)).ToArray();

            var parameters = pars.Where(param => param is not OptionInfo).ToArray();

            var options = pars.Where(param => param is OptionInfo).Cast<OptionInfo>().ToArray();

            return (parameters, options);

        }
        private static string[] GetNameVariants(string name, NamingStyle styleFlags)
        {
            bool shouldLowercase = styleFlags.HasFlag(NamingStyle.CaseInsensitive);
            var cleaned = name.Replace("@", "");

            // Find word boundary positions (where separators should be inserted)
            var splitPositions = new List<int>();
            if (!cleaned.All(char.IsUpper)) // all-upper means single word (e.g. "URL")
            {
                for (int i = 1; i < cleaned.Length; i++)
                {
                    if (char.IsUpper(cleaned[i]))
                        splitPositions.Add(i);
                }
            }

            if (splitPositions.Count == 0)
            {
                // Single word — no separators possible
                return new[] { shouldLowercase ? cleaned.ToLower() : cleaned };
            }

            var separators = new List<(NamingStyle flag, char? sep)>
            {
                (NamingStyle.KebabCase, '-'),
                (NamingStyle.SnakeCase, '_'),
                (NamingStyle.DotCase, '.'),
                (NamingStyle.NoSeparator, null),
            };

            var variants = new List<string>();
            foreach (var (flag, sep) in separators)
            {
                if (!styleFlags.HasFlag(flag))
                    continue;
                var sb = new StringBuilder();
                int prev = 0;
                foreach (var pos in splitPositions)
                {
                    if (sb.Length > 0 && sep.HasValue)
                        sb.Append(sep.Value);
                    sb.Append(cleaned, prev, pos - prev);
                    prev = pos;
                }
                if (sb.Length > 0 && sep.HasValue)
                    sb.Append(sep.Value);
                sb.Append(cleaned, prev, cleaned.Length - prev);

                var result = sb.ToString();
                // Separated variants are always lowercase; NoSeparator is lowercase only when CaseInsensitive
                if (sep.HasValue || shouldLowercase)
                    result = result.ToLower();
                variants.Add(result);
            }

            if (variants.Count == 0)
                variants.Add(shouldLowercase ? cleaned.ToLower() : cleaned);

            return variants.ToArray();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(ctx =>
            {
                var asm = Assembly.GetExecutingAssembly();
                using var stream = asm.GetManifestResourceStream("CommandLineArgsGenerator.StyleEnums.cs");
                using var reader = new System.IO.StreamReader(stream);
                ctx.AddSource("StyleEnums.g.cs", reader.ReadToEnd());
            });
            context.RegisterForSyntaxNotifications(() => new ParserSyntaxReceiver());
        }
    }
}
