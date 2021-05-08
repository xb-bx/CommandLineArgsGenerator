namespace CommandLineArgsGenerator
{
    public interface ICommandInfo 
	{
        public string Name { get; }
        public string HelpText { get; } 
        public string RawName { get; set; }
	}
} 
