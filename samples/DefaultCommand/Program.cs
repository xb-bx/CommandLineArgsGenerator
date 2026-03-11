using System;
using CommandLineArgsGenerator.Styles;

namespace DefaultCommand;

[App(HelpCommand = null, SkipCommandParsing = true)]
public static class Program
{
    /// <summary default="true">
    /// This command is invoked when the user doesn't specify any sub command.
    /// </summary>
    [DefaultCommand]
    public static int DefaultCommand(string param, bool flag = false)
    {
        Console.WriteLine($"DefaultCommand {param} {(flag ? "true" : "")}");
        return 0;
    }

    public enum EnumParam
    {
        EnumValue1,
        EnumValue2,
        EnumValue3,
        EnumValue4,
        EnumValue5
    }

    /// <summary>
    /// This command is to test optional parameters syntax
    /// </summary>
    /// <param name="mandatory1" alias="m">mandatory string value</param>
    /// <param name="mandatory2" alias="n">mandatory int value</param>
    /// <param name="arrayParam">array of strings</param>
    /// <param name="intParam" alias="i">optional int value</param>
    /// <param name="enumParam">valid values: EnumValue1, EnumValue2, ... EnumValue5 </param>
    /// <returns></returns>
    public static void OptionalArgsTest(string mandatory1, int mandatory2, string[]? arrayParam , int? intParam = null,
        EnumParam? enumParam = null)
    {
        Console.WriteLine($"Invoked {mandatory1}-{mandatory2} with intParam={intParam}, enumParam={enumParam}, arrayParam={string.Join(",",arrayParam ?? new string[] {})}");
    }
    
    public static void AnotherCommand() { }
}