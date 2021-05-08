using Microsoft.CodeAnalysis;

namespace CommandLineArgsGenerator
{
    [Generator]
    public class AttributesGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            SourceBuilder sb = new SourceBuilder();

            sb
                .Using("System")
                .Namespace((context.SyntaxReceiver as NamespaceSyntaxReceiver).Namespace)
                    .Class("AppAttribute", "Attribute") 
                    .Close()
                    .Class("DefaultAttribute", "Attribute")
                    .Close()
                    .Class("IgnoreAttribute", "Attribute")
                    .Close() 
                    .Interface("IArgumentConverter<T>")
                        .RawCode("T Convert(string str);")
                    .Close()
                .Close(); 
            context.AddSource("Attributes.cs", sb.ToString());
                //System.IO.File.WriteAllText("/home/jj/log.cs", sb.ToString());

        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new NamespaceSyntaxReceiver());
        }
    }

} 
