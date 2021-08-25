using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System;
namespace CommandLineArgsGenerator
{
    public struct HelpText
	{
		private Dictionary<string, string> texts = new();
		public string? this[string culture]
		{
			get 
			{
				if(texts.ContainsKey(culture))
					return texts[culture];
				else 
					return null;
			}
			set 
			{
				if(texts.ContainsKey(culture))
					texts[culture] = value!;
				else
					texts.Add(culture, value!);
			}
		}
		public static HelpText FromXElement(XElement elem, string defaultCulture) 
		{
			HelpText help = new();
			foreach(var node in elem.Nodes())
			{
				if(node.NodeType == XmlNodeType.Text) 
				{
					help[defaultCulture] = (node as XText).Value.Trim();
				}
				else
				{
					var el = node as XElement;
					help[el.Name.ToString()] = el.Value.Trim();
				}
			}
			return help;
		}
	}
} 
