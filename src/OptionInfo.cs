using Microsoft.CodeAnalysis;
namespace CommandLineArgsGenerator
{
    public class OptionInfo : ParameterInfo
    {
        public string? Default { get; set; }
        public string? Alias { get; set; }
        public bool IsArray { get; set; }
        public bool IsBool => Type?.SpecialType == SpecialType.System_Boolean;
    }

} 
