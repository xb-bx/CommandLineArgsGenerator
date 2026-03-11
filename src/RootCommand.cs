#nullable disable
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CommandLineArgsGenerator.Styles;

namespace CommandLineArgsGenerator
{
    public class RootCommand : CommandInfoBase
    {
        public ClassDeclarationSyntax Class { get; set; }
		public List<CommandInfoBase> Children { get; set; }
        public CommandInfoBase Default { get; set; }
        public NamingStyle ParamStyle { get; set; } = NamingStyle.AllVariants;
        public NamingStyle EnumStyle { get; set; } = NamingStyle.AllVariants;
        public SeparatorStyle ValueSeparator { get; set; } = SeparatorStyle.All;
        public PrefixStyle ArgPrefix { get; set; } = PrefixStyle.All;
        public MandatoryStyle MandatoryStyle { get; set; } = MandatoryStyle.Mixed;
        public bool SkipCommandParsing { get; set; }
        public string? HelpCommand { get; set; } = "help";
	}
}
