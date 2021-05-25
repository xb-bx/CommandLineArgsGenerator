using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CommandLineArgsGenerator
{
    public class NamespaceSyntaxReceiver : ISyntaxReceiver
    {
        public string? Namespace { get; set; }
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if(Namespace is null && syntaxNode is NamespaceDeclarationSyntax @namespace)
            {
                Namespace = @namespace.Name.ToString();
            }
        }
    }
} 
