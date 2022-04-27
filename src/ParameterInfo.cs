#nullable disable
using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CommandLineArgsGenerator
{
    public class ParameterInfo
    {
        public string Name { get; set; }
        public string RawName { get; set; }
        public INamedTypeSymbol Type { get; set; }
        public HelpText? HelpText { get; set; } 
        public bool IsNullable { get; set; }
        
        private string displayTypeName;
        public string DisplayTypeName 
        { 
            get 
            { 
                return displayTypeName;
            } 
            set 
            {
                displayTypeName = value;
                UnderscoredTypeName = displayTypeName.Replace('.', '_');
            } 
        }
        public string UnderscoredTypeName { get; set; }
		public bool IsString => Type?.SpecialType == SpecialType.System_String;
		public bool IsEnum => Type?.BaseType?.SpecialType == SpecialType.System_Enum;



		public bool HasMethod(string name, int arguments) 
		{
			if(Type is null)
				return false;
			return Type
					.GetMembers(name)
					.Where(member => (member as IMethodSymbol) != null)
					.Cast<IMethodSymbol>()
					.FirstOrDefault(method => method.Parameters.Length == arguments) != null;
		}
		public bool HasCtor(string[] parameters)
        {
			if (Type is null)
				return false;
			return Type
					.Constructors
					.FirstOrDefault(ct => ct.Parameters.Length == parameters.Length
								&& ct.Parameters
									.Zip(parameters, (p, t) => (p, t)).All(p => p.p.Type.ToString() == p.t.ToString())) != null;
        }
		
    }
} 
