using System;
using System.Text;
namespace CommandLineArgsGenerator 
{
    public class IntentedWriter
    {
        private StringBuilder sb = new();
        public int Level { get; set; }
        public IntentedWriter Clear()
        {
            sb.Clear();
            return this;
        }
        public IntentedWriter Write(string str, bool escape =false)
        {
            string tab = escape ? "\\t" : "\t";
            for(int i = 0; i < Level; i++)
            {
                sb.Append(tab);
            }
            sb.Append(str);
            return this;
        }
        public IntentedWriter WriteLine(string str, bool escape = false)
        {
            string tab = escape ? "\\t" : "\t";

            for(int i = 0; i < Level; i++)
            {
                sb.Append(tab);
            }
            if(escape)
            {
                sb.Append(str);
                sb.Append("\\n");
            }
            else
            {
                sb.AppendLine(str);
            }
            return this;
        }
        public override string ToString()
            => sb.ToString();
    }
}