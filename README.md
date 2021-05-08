# CommandLineArgsGenerator
Source generator to generate command line args parser.   
The parser will be generated at compile-time that means ***NO REFLECTION!***.
## Usage:
```cs
[App] // Necessary auto generated attribute
public class Program
{
  ///<summary>This will be help text for command</summary>
  ///<param name="str">This will be help text for param</param>
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
## Type conversation
***Supported any type*** that has static method Parse(string) or constructor with one argument of type string.
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
## Install
```
dotnet add package CommandLineArgsGenerator
```
