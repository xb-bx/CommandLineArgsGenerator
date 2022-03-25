using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace CommandLineArgsGenerator
{
    public class ParserSyntaxReceiver : ISyntaxContextReceiver
    {
        public RootCommand? Root { get; private set; }
        public string? Namespace { get; private set; }
        public Dictionary<string, string> Converters { get; private set; } = new();
        private static SymbolDisplayFormat typeFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);


        private RootCommand? GetClass(SyntaxNode node, SemanticModel semanticModel)
        {
            if (node is ClassDeclarationSyntax cl)
            {
                foreach (var item in cl.AttributeLists)
                {
                    foreach (var attr in item.Attributes)
                    {
                        if (attr.Name.ToString() == "App" || attr.Name.ToString() == "AppAttribute")
                        {
                            var root = new RootCommand { Class = cl, Name = null };
                            return root;
                        }
                    }
                }
                if (cl.BaseList is not null)
                {
                    foreach (var item in cl.BaseList.Types.Select(x => semanticModel.GetTypeInfo(x.Type)).Where(x => x.Type?.Name == "IArgumentConverter"))
                    {
                        var target = (item.Type as INamedTypeSymbol)!.TypeArguments[0].ToDisplayString(typeFormat);
                        var name = GetFullName(cl);
                        if(Namespace is not null)
                        {              
                            if(name.StartsWith(Namespace))
                            {
                                name = name.Substring(Namespace.Length + 1);
                            }
                        }
                        
                        Converters.Add(target, name);
                    }
                }
            }
            else if(node is NamespaceDeclarationSyntax @namespace && Namespace is null)
            {
                    Namespace = @namespace.Name.ToString();
            }
            else if(node is FileScopedNamespaceDeclarationSyntax ns)
            {
                var nsstring = ns.Name.ToString();
                if(Namespace is null || Namespace.StartsWith(nsstring))
                    Namespace = nsstring;
            }
            return null;
        }


        public static string GetFullName(MemberDeclarationSyntax node)
        {
            var sb = new StringBuilder();
            SyntaxNode current = node;
            while (current is not null)
            {
                if (current is NamespaceDeclarationSyntax nds)
                {
                    sb.Insert(0, '.');
                    sb.Insert(0, nds.Name.ToString());
                    current = nds.Parent!;
                }
                else if (current is ClassDeclarationSyntax cds)
                {
                    sb.Insert(0, '.');
                    sb.Insert(0, cds.Identifier.ToString());
                    current = cds.Parent!;
                }
                else if (current is MethodDeclarationSyntax mds)
                {
                    sb.Insert(0, '.');
                    sb.Insert(0, mds.Identifier.ToString());
                    current = mds.Parent!;
                }
                else
                {
                    break;
                }
            }
            return sb.Remove(sb.Length - 1, 1).ToString();
        }





        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var cl = GetClass(context.Node, context.SemanticModel);
            if (Root is null && cl is not null)
            {
                Root = cl;
            }
        }

    }

}
