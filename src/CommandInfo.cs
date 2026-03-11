#nullable disable
using System.Collections.Generic;
using System.Text;

namespace CommandLineArgsGenerator
{
    public class CommandInfo : CommandInfoBase
    {
        public string NameInSourceCode { get; set; }
        public ParameterInfo[] Parameters { get; set; }
        public OptionInfo[] Options { get; set; }
        public bool IsTask { get; set; }
        public override bool Equals(object obj)
        {
            return obj is CommandInfo info &&
                   RawName == info.RawName;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(RawName);
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(RawName);
            foreach(var p in Parameters)
            {
                sb.Append(" <");
                sb.Append(p.RawName);
                sb.Append("> ");
            }
            foreach (var p in Options)
            {
                sb.Append(" -");
                sb.Append(p.RawName);
            }
            return sb.ToString();
        }
    }

} 
