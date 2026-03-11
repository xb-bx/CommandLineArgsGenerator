using System;
using CommandLineArgsGenerator.Styles;
namespace NAMESPACE
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AppAttribute : Attribute
    {
        public NamingStyle ParamStyle { get; set; } = NamingStyle.AllVariants;
        public NamingStyle EnumStyle { get; set; } = NamingStyle.AllVariants;
        public SeparatorStyle ValueSeparator { get; set; } = SeparatorStyle.All;
        public PrefixStyle ArgPrefix { get; set; } = PrefixStyle.All;
        public MandatoryStyle MandatoryStyle { get; set; } = MandatoryStyle.Mixed;
        public bool SkipCommandParsing { get; set; } = false;
        public string HelpCommand { get; set; } = "help";
    }
}