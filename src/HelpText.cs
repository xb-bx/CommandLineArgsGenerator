using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System;
namespace CommandLineArgsGenerator
{
    public struct HelpText
    {
        private Dictionary<string, string> texts;
        public Dictionary<string, string> Texts => texts ??= new(); 
        public bool IsEmpty => Texts.Count == 0;
        public string? this[string lang]
        {
            get
            {
				texts ??= new();
                if (texts.ContainsKey(lang))
                    return texts[lang];
                else
                    return null;
            }
            set
            {
				texts ??= new();
                if (texts.ContainsKey(lang))
                    texts[lang] = value!;
                else
                    texts.Add(lang, value!);
            }
        }
        public static HelpText FromXElement(XElement elem, string defaultLanguage)
        {
            HelpText help = new ();
            foreach (var node in elem.Nodes())
            {
                if (node.NodeType == XmlNodeType.Text)
                {
                    help[defaultLanguage] = (node as XText).Value.Trim().Replace("\n", "\\n");
                }
                else
                {
                    var el = node as XElement;
                    help[el.Name.ToString()] = el.Value.Trim().Replace("\n", "\\n");
                }
            }
            return help;
        }
    }
}
