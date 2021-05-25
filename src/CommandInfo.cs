#nullable disable
using System.Collections.Generic;
using System.Text;

namespace CommandLineArgsGenerator
{
    public class CommandInfo : ICommandInfo
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string RawName { get; set; }
        public string NameInSourceCode { get; set; }
        public string HelpText { get; set; }
        public ParameterInfo[] Parameters { get; set; }
        public OptionInfo[] Options { get; set; }

        public override bool Equals(object obj)
        {
            return obj is CommandInfo info &&
                   Name == info.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            foreach(var p in Parameters)
            {
                sb.Append(" <");
                sb.Append(p.Name);
                sb.Append("> ");
            }
            foreach (var p in Options)
            {
                sb.Append(" -");
                sb.Append(p.Name);
            }
            return sb.ToString();
        }
    }

} 
