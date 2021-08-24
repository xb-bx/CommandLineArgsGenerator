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
	[AttributeUsage(AttributeTargets.Class)]
    public class AppAttribute : Attribute 
    {{
        
    }}
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DefaultAttribute : Attribute 
    {{
        
    }}
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
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
