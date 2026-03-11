using System;

namespace CommandLineArgsGenerator.Styles
{
    [Flags]
    public enum NamingStyle
    {
        KebabCase       = 1,
        SnakeCase       = 2,
        DotCase         = 4,
        NoSeparator     = 8,
        CaseInsensitive = 16,
        AllVariants     = KebabCase | SnakeCase | DotCase | NoSeparator | CaseInsensitive
    }

    [Flags]
    public enum SeparatorStyle
    {
        Whitespace = 1,
        Colon      = 2,
        Equals     = 4,
        All        = Whitespace | Colon | Equals
    }

    [Flags]
    public enum PrefixStyle
    {
        Slash        = 1,
        Hyphen       = 2,
        DoubleHyphen = 4,
        All          = Slash | Hyphen | DoubleHyphen
    }

    public enum MandatoryStyle
    {
        Mixed      = 0,
        Positional = 1,
        Named      = 2,
    }
}
