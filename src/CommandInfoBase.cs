namespace CommandLineArgsGenerator
{
    public abstract class CommandInfoBase 
	{
        public string Name { get; set; }
        public virtual HelpText? HelpText { get; set; }
        public string RawName { get; set; }
		private string fullName;
        public string FullName 
		{ 
			get => fullName;  
			set {
				fullName = value;
				UnderscoredName = fullName.Replace(" ", "_").Replace('-', '_');
			}
		}
		public string UnderscoredName { get; set; }
		
	}
} 
