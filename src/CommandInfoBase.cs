namespace CommandLineArgsGenerator
{
    public abstract class CommandInfoBase 
	{
        public string Name { get; set; }
        public HelpText? HelpText { get; set; }
        public string RawName { get; set; }
        public string FullName { get; set; }
	}
} 
