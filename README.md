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
There will be one command 'file' with two subcommands 'create' and 'delete'

### Shell autocompletion
Generator automaticaly creates command 'complete' that accepts cursor position and argument list. 
Using this command you can create autocompletion script for your shell.

### Tasks
Methods that returns `Task` or `Task<T>` are supported
### Default commands
You can mark method or class with setting summary's attribute default to "true" 
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
## Install
```
dotnet add package CommandLineArgsGenerator
```
