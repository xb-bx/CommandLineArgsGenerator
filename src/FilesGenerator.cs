using Microsoft.CodeAnalysis;
using System.Linq;
using System.Reflection;
namespace CommandLineArgsGenerator
{
    [Generator]
    public class FilesGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        { 
            var asm = Assembly.GetExecutingAssembly();
			var manifestNames = asm.GetManifestResourceNames();
            var csSources =
                manifestNames
                    .Where(name => name.EndsWith(".cs"));
            var fileNames = 
                csSources
                    .Select(name => name.Replace("CommandLineArgsGenerator.templates.", ""));
                    
            foreach(var (cs, fileName) in csSources.Zip(fileNames, (c, f) => (c, f)))
            {
				using var stream = asm.GetManifestResourceStream(cs);
				using var reader = new System.IO.StreamReader(stream);
                var content = reader.ReadToEnd().Replace("NAMESPACE", (context.SyntaxReceiver as NamespaceSyntaxReceiver)!.Namespace!);
                context.AddSource(fileName, content);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new NamespaceSyntaxReceiver());
        }
    }

} 
