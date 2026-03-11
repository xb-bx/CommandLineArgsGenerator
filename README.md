# CommandLineArgsGenerator
Source generator to generate command line args parser.   
The parser will be generated at compile-time that means ***NO REFLECTION!***.
## Usage:
```cs
[App] // Necessary auto generated attribute
public class Program
{
  ///<summary>This will be help text for command</summary>
  ///<param name="str" alias="s">This will be help text for param.</param>
  public static void MyCoolCommand(string str, int num, FileInfo file, /* color is optional parameter */ ConsoleColor color = ConsoleColor.Grey)
  {
    Console.ForegroundColor = color
    Console.WriteLine(str);
    Console.WriteLine(num);
    Console.WriteLine(file.FullPath);
  }
}
```
Compile and run and you will see following output:
```
Usage: <command> <parameters> [options]
Commands:
        my-cool-command: This will be help text for command
```
Nested commands:
```cs
[App] // Necessary auto generated attribute
public class Program
{
  public class File 
  {
    public static void Create(FileInfo file)
    {
      ...
    }
    public static void Delete(FileInfo file)
    {
      ...
    }
  }
}
```
There will be one command 'file' with two subcommands 'create' and 'delete'.
Methods, marked with `[Ignore]` will not be used as commands.

### Tasks
Methods that returns `Task` or `Task<T>` are supported

### Default commands
If only one command exists in a file - then it is **default** command (you don't need to specify it's name to run it). 
In other case **default** command can be specified by:
1. **`[DefaultCommand]` attribute**: Apply `[DefaultCommand]` to a method or nested class.
2. **XML doc**: Set `<summary default="true">` on the method or class (can have no effect on "generated for viewing" code in some IDE's)

```cs
[App]
public class Program
{
    // This is the only method, so it's automatically the default command.
    public static void Run(string name, int count = 1) { ... }
}
```

```cs
[App]
public class Program
{
    /// <summary default="true">"default" can be also used to mark default command</summary>    
    [DefaultCommand]
    public static void Run(string name) { ... }

    public static void OtherCommand(string arg) { ... }
}
```
### Argument syntax
Supported command and argument names can be in `kebab-case`, `snake_case`, `dot.case`, all case insensitive(configurable). Supported argument prefixes `/`, `-` and `--` (configurable). Supported name value separators are `:`, `=` and whitespace (configurable).
```sh
> myapp MyFancyCommand --MyFancyOption 123  #OK - Case insensitive matching
> myapp my-fancy-command /my-fancy-option:123  #OK - kebab-case, '/' and ':'
> myapp my_fancy_command -my-fancy-option=123  #OK - snake_case, '-' and '='
> myapp my.fancy.command --my.fancy.option 123  #OK - dot.case, '--' and ' '
```

### Mandatory and optional parameters
Mandatory parameters (method arguments without default values) can be passed **positionally** without name or **by name** (in any order) - using prefixes `/`, `-`, `--`.
Optional parameters (options) can only be passed **by name**:
```cs
public static void Greet(string name, int times, int maybe = 0) { ... }
```
```sh
> myapp greet John 3                     # positional mandatory
> myapp greet John 3 --maybe 5           # positional mandatory with option
> myapp greet John 3 5                   # options require names - will emit "Unexpected argument 5" error
> myapp greet --times=3 --name=John      # named mandatory, reversed order
> myapp greet John --times=3             # mixed positional + named
> myapp greet --times 3 --maybe 5        # name missing - will emit error
```
If a mandatory parameter is missing, the parser prints an error:
```
Missing required parameter 'name'
```

### Parameter aliases
Both mandatory and optional parameters support short aliases via the `alias` xmldoc attribute:
```cs
/// <param name="name" alias="n">User name</param>
/// <param name="output" alias="o">Output file</param>
public static void Greet(string name, string output = "out.txt") { ... }
```
```
> myapp greet -n=John -o=result.txt
> myapp greet -n John                # space-separated
```
Aliases use single-character prefixes (`-`, `/`) and appear in help text (e.g. `-n|name`).

### Shell autocompletion
Generator automaticaly creates command 'complete' that accepts cursor position and argument list. 
Using this command you can create autocompletion script for your shell.

**Disabled by default. To enable this feature insert `<GenerateCompletion>True</GenerateCompletion>` into your .csproj**
### Command suggestions 
If you mistyped command or option name it will print suggestion "Did you mean "command-name?"".

**Disabled by default. To enable this feature insert `<GenerateSuggestions>True</GenerateSuggestions>` into your .csproj**
## Type convertion
***Supported any type*** that has static method Parse(string) or TryParse(string, out T) or with constructor with one argument of type string.
For example, int or FileInfo.
### Custom type convertor
To convert custom type without string constructor or method Parse, you must create class that implements `IArgumentConverter<T>`
#### Example:
```cs
public struct Point
{
  public int X {get; private set;}
  public int Y {get; private set;}
  
  public Point(int x, int y)
  {
    X = x;
    Y = y;
  }
}
public class PointConverter : IArgumentConverter<Point> 
{
  public Point Convert(string str)
  {
    var splited = str.Split('x', 2); // The string must looks like '15x25'
    return new Point(int.Parse(splited[0]), int.Parse(splited[1])); 
  }
}
```
### Native AOT
Because of absence of reflection you can compile your app using [NativeAOT](https://github.com/dotnet/runtimelab/tree/feature/NativeAOT/) to make it more faster
### Arrays
**Arrays always will be as optionals**
### Localization
You can use following constructions to get multi-language help texts:
```xml
<summary>
    <en>Text in English</en>
    <ru>Текст на русском</ru>
</summary>
```
It depends on current culture's `TwoLetterISOLanguageName` property.
Also you can specify default language by setting project's property `DefaultLanguage`(by default it's 'en')
### If you want to checkout generated parser add the following to your .csproj:
```xml
<PropertyGroup>
    <LogGeneratedParser>Path-to-parser-dir</LogGeneratedParser>
</PropertyGroup>
```

### Other [App] parameters
The `[App]` attribute accepts optional properties to control how arguments are parsed. All default to accepting all variants for maximum flexibility.

### Argument prefixes
Control which prefixes are accepted for options and named parameters via `ArgPrefix`:
```cs
[App(ArgPrefix = PrefixStyle.DoubleHyphen | PrefixStyle.Slash)] // accepts --option and /option, but not -option
```
Available flags: `DoubleHyphen` (`--`), `Hyphen` (`-`), `Slash` (`/`). Default: `All`.
```
> myapp my-command --name=John /age=30 -v
```

### Value separators
Control how values are separated from option/parameter names via `ValueSeparator`:
```cs
[App(ValueSeparator = SeparatorStyle.Equals | SeparatorStyle.Whitespace)] // --name=John or --name John, but not --name:John
```
Available flags: `Whitespace` (` `), `Colon` (`:`), `Equals` (`=`). Default: `All`.

### Naming styles
Control accepted naming conventions for parameters, options, and enum values via `ParamStyle` and `EnumStyle`:
```cs
[App(ParamStyle = NamingStyle.KebabCase | NamingStyle.CaseInsensitive, EnumStyle = NamingStyle.NoSeparator)]
```
Available flags: `KebabCase` (`my-param`), `SnakeCase` (`my_param`), `DotCase` (`my.param`), `NoSeparator` (`myparam`), `CaseInsensitive`. Default: `AllVariants` (all of the above).

For a method parameter `myParam`:
```
--my-param=val   # KebabCase
--my_param=val   # SnakeCase
--my.param=val   # DotCase
--myparam=val    # NoSeparator
--MY-PARAM=val   # CaseInsensitive (combinable with any of the above)
```
### Mandatory parameter style
Control how mandatory parameters are passed via `MandatoryStyle`:
```cs
[App(MandatoryStyle = MandatoryStyle.Positional)]  // positional only
[App(MandatoryStyle = MandatoryStyle.Named)]        // named only (--param=value)
[App(MandatoryStyle = MandatoryStyle.Mixed)]        // both (default)
```
- **Mixed** (default): parameters can be passed positionally or by name — current behavior, fully backward compatible.
- **Positional**: parameters can only be passed positionally (`command val1 val2`). Named `--param` case labels are not generated.
- **Named**: parameters must be passed by name (`command --param=value`). Positional fallback is disabled — unmatched arguments produce an error.

#### SkipCommandParsing
When a default command has no sibling commands, the generated parser normally still tries to match the command name.
```cs
[App]
public class Program
{
    public static void Message(string msg, bool flag = false) { Console.WriteLine($"Sending \"{msg}\" ({flag})") }
}
```
```sh
> myapp text --flag
Sending "text" (True)

> myapp message --flag     # "message" is treated as command name, as it matches one
Error: Missing required parameter 'msg'
```
Set `SkipCommandParsing = true` to skip this — all arguments go directly to parameter/option parsing:
```cs
[App(SkipCommandParsing = true)]
```
```sh
> myapp message --flag    # "myfile.txt" is parsed as the 'file' parameter, not as a command name
Sending "message" (True)
```
Combine with overriden `HelpCommand` to pass really any string as parameter.
#### HelpCommand
By default the help command word is `"help"`. You can change it or disable it entirely:
```cs
[App(HelpCommand = "info")]   // use "info" instead of "help"
```
The parser also accepts prefixed variants at the root level (e.g. `--help`, `-help`, `/help` or `--info`, etc.).
When disabled (`""` or null), no help case is generated — then any first string (including "help") treated as a positional parameter to a default command.
```sh
> myapp help --flag
Usage: [command] <parameters> [options] ....
```
```cs
[App(HelpCommand = null)]       // disable help command entirely
```
```sh
> myapp help --flag
Sending "help" (True)    # "help" is used as first parameter 
```
## Install
```
dotnet add package CommandLineArgsGenerator
```
