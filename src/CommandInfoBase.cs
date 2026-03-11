namespace CommandLineArgsGenerator
{
    public abstract class CommandInfoBase
	{
        public string[] NameVariants { get; set; }
        public string[] FullNameVariants { get; set; }
        public virtual HelpText? HelpText { get; set; }
        public string RawName { get; set; }
		public string UnderscoredName { get; set; }

	}
}
