#nullable disable
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CommandLineArgsGenerator
{
    public class RootCommand : CommandInfoBase
    {
        public ClassDeclarationSyntax Class { get; set; }     
		public List<CommandInfoBase> Children { get; set; }
        public CommandInfoBase Default { get; set; }
	}
} 
