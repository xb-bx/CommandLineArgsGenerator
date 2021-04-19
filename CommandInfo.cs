namespace CommandLineArgsGenerator
{
    public class CommandInfo
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string HelpText { get; set; }
        public ParameterInfo[] Parameters { get; set; }
        public OptionInfo[] Options { get; set; }
    }

} 
