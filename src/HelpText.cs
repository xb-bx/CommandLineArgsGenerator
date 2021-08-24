using System.Collections.Generic;
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
	}
} 
