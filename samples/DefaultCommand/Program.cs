namespace DefaultCommand;

[App]
public static class Program
{
    /// <summary default="true">
    /// This command is invoked when the user doesn't specify any sub command.
    /// </summary>
    public static int DefaultCommand() => 0;
}