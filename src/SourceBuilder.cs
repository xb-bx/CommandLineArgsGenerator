using System.Text;

namespace CommandLineArgsGenerator
{
    public class SourceBuilder
    {
        private StringBuilder sb = new StringBuilder();
        private int currentIndentLevel = 0;
        public SourceBuilder Using(string @namespace)
        {
            sb.AppendLine($"using {@namespace};");
            return this;
        }
        public SourceBuilder Class(string name, string inherited = null)
        {
            if(currentIndentLevel > 0)
            {
                sb.Append('\t', currentIndentLevel);
            }
            sb.Append($"public class {name} ");
            if(inherited != null)
            {
                sb.AppendLine($": {inherited}");
                if (currentIndentLevel > 0)
                {
                    sb.Append('\t', currentIndentLevel);
                }
                sb.AppendLine("{");
            }
            else
            {
                sb.AppendLine("{");
            }
            currentIndentLevel++;
            return this;
        }
        public SourceBuilder Interface(string name, string inherited = null)
        {
            if (currentIndentLevel > 0)
            {
                sb.Append('\t', currentIndentLevel);
            }
            sb.Append($"public interface {name} ");
            if (inherited != null)
            {
                sb.AppendLine($": {inherited}");
                if (currentIndentLevel > 0)
                {
                    sb.Append('\t', currentIndentLevel);
                }
                sb.AppendLine("{");
            }
            else
            {
                sb.AppendLine("{");
            }
            currentIndentLevel++;
            return this;
        }
        public SourceBuilder Close()
        {
            if (currentIndentLevel > 1)
            {
                sb.Append('\t', --currentIndentLevel);
            }
            sb.AppendLine("}");
            return this;
        }
        public SourceBuilder Start()
        {
            if (currentIndentLevel > 0)
            {
                sb.Append('\t', currentIndentLevel);
            }
            sb.AppendLine("{");
            currentIndentLevel++;
            return this;
        }
        public SourceBuilder Method(string name, string returnType, string modifiers, params string[] args)
        {
            if (currentIndentLevel > 0)
            {
                sb.Append('\t', currentIndentLevel);
            }
            sb.Append($"{modifiers} {returnType} {name} (");
            sb.Append(string.Join(",", args));
            sb.AppendLine(")");
            if (currentIndentLevel > 0)
            {
                sb.Append('\t', currentIndentLevel);
            }
            sb.AppendLine("{");
            currentIndentLevel++;
            return this;
        }
        public SourceBuilder AutoProp(string name, string type)
        {
            if (currentIndentLevel > 0)
            {
                sb.Append('\t', currentIndentLevel);
            }
            sb.AppendLine($"public {type} {name} {{ get; private set; }}");
            return this;
        }
        public SourceBuilder Field(string name, string type)
        {
            if (currentIndentLevel > 0)
            {
                sb.Append('\t', currentIndentLevel);
            }
            sb.AppendLine($"private {type} {name};");
            return this;
        }
        public SourceBuilder RawCode(string code)
        {
            if (currentIndentLevel > 0)
            {
                sb.Append('\t', currentIndentLevel);
            }
            sb.AppendLine(code);
            return this;
        }
        public SourceBuilder Namespace(string @namespace)
        {
            if (currentIndentLevel > 0)
            {
                sb.Append('\t', currentIndentLevel);
            }
            sb.AppendLine($"namespace {@namespace} ");
            if (currentIndentLevel > 0)
            {
                sb.Append('\t', currentIndentLevel);
            }
            sb.AppendLine("{");
            currentIndentLevel++;
            return this;
        }
        public SourceBuilder Ctor(string className,params string[] args)
        {
            if (currentIndentLevel > 0)
            {
                sb.Append('\t', currentIndentLevel);
            }
            sb.Append($"public {className} (");
            sb.Append(string.Join(",", args));
            sb.AppendLine(")");
            if (currentIndentLevel > 0)
            {
                sb.Append('\t', currentIndentLevel);
            }
            sb.AppendLine("{");
            currentIndentLevel++;
            return this;
        }
        public override string ToString()
        {
            return sb.ToString();
        }
    } 
} 