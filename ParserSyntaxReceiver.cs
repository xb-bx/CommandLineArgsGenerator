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
        public ClassDeclarationSyntax Class { get;private set; } 
        public Dictionary<string, string> Converters { get; private set; } = new();

        private static SymbolDisplayFormat typeFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);


        private ClassDeclarationSyntax GetClass(SyntaxNode node, SemanticModel semanticModel)
        {
            if (node is ClassDeclarationSyntax cl)
            {
                foreach (var item in cl.AttributeLists)
                {
                    foreach (var attr in item.Attributes)
                    {
                        if (attr.Name.ToString() == "App" || attr.Name.ToString() == "AppAttribute")
                        {
                            return cl;
                        }
                    }
                }
                if (cl.BaseList is not null)
                {
                    foreach (var item in cl.BaseList.Types.Select(x => semanticModel.GetTypeInfo(x.Type)).Where(x => x.Type?.Name == "IArgumentConverter"))
                    {
                        Converters.Add((item.Type as INamedTypeSymbol).TypeArguments[0].ToDisplayString(typeFormat), GetFullName(cl));
                    }
                }
            }

            return null;
        }



        public static string GetFullName(ClassDeclarationSyntax @class)
        {
            var sb = new StringBuilder();
            SyntaxNode current = @class;
            while (current is not null)
            {
                if(current is NamespaceDeclarationSyntax nds)
                {
                    sb.Insert(0, '.');
                    sb.Insert(0, nds.Name.ToString());
                    current = nds.Parent;
                }
                else if(current is ClassDeclarationSyntax cds)
                {
                    sb.Insert(0, '.');
                    sb.Insert(0, cds.Identifier.ToString());
                    current = cds.Parent;
                }
                else
                {
                    break;
                }
            }
            return sb.Remove(sb.Length-1, 1).ToString();
        }




        
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var cl = GetClass(context.Node, context.SemanticModel);
            if (cl is not null)
            {
                Class = cl; 
            }
        }

    }

}
