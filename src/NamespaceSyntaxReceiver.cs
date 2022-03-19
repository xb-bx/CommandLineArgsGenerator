using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CommandLineArgsGenerator
{
    public class NamespaceSyntaxReceiver : ISyntaxReceiver
    {
        public string? Namespace { get; set; }
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if(Namespace is null)
            {
                if(syntaxNode is NamespaceDeclarationSyntax @namespace)
                    Namespace = @namespace.Name.ToString();
                else if(syntaxNode is FileScopedNamespaceDeclarationSyntax ns)
                    Namespace = ns.Name.ToString();
            }
        }
    }
} 
