using Microsoft.CodeAnalysis;

namespace CommandLineArgsGenerator
{
    [Generator]
    public class AttributesGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            string attributes = 
$@"
using System;
namespace {(context.SyntaxReceiver as NamespaceSyntaxReceiver)!.Namespace!} 
{{
    public class AppAttribute : Attribute 
    {{
        
    }}
    public class DefaultAttribute : Attribute 
    {{
        
    }}
    public class IgnoreAttribute : Attribute 
    {{
        
    }}
    public interface IArgumentConverter<T> 
    {{
        T Convert(string str);
    }}
}}
";      context.AddSource("Attributes.cs", attributes);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new NamespaceSyntaxReceiver());
        }
    }

} 
