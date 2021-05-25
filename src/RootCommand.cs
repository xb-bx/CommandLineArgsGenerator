#nullable disable
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CommandLineArgsGenerator
{
    public class RootCommand : ICommandInfo
    {
        public ClassDeclarationSyntax Class { get; set; } 
        
		public List<ICommandInfo> Children { get; set; }
        public ICommandInfo Default { get; set; }
		public string Name { get; set; }
        public string RawName { get; set; }
		public string FullName { get; set; }
        public string HelpText { get; set; }
	}
} 
