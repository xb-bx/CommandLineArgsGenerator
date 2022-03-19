using System;
using System.IO;

namespace Converter;

[App]
public class Program
{
    ///<summary>Converts decimal number to some another number system</summary>
    public class To
    {
        ///<summary>Converts decimal number to binary number</summary>
        ///<param name="number">Number to convert</param>
        public static void Bin(long number)
        {
            Console.WriteLine(Convert.ToString(number, 2));
        }

        ///<summary>Converts decimal number to octal number</summary>
        ///<param name="number">Number to convert</param>
        public static void Octo(long number)
        {
            Console.WriteLine(Convert.ToString(number, 8));
        }

        ///<summary>Converts decimal number to hex number</summary>
        ///<param name="number">Number to convert</param>
        public static void Hex(long number)
        {
            Console.WriteLine(Convert.ToString(number, 16));
        }
    }

    ///<summary>Converts to decimal number from some another number system</summary>
    public class From
    {
        ///<summary>Converts binary number to decimal number</summary>
        ///<param name="number">Number to convert</param>
        public static void Bin(string number)
        {
            Console.WriteLine(Convert.ToInt64(number, 2));
        }

        ///<summary>Converts octal number to decimal number</summary>
        ///<param name="number">Number to convert</param>
        public static void Octo(string number)
        {
            Console.WriteLine(Convert.ToInt64(number, 8));
        }

        ///<summary>Converts hex number to decimal number</summary>
        ///<param name="number">Number to convert</param>
        public static void Hex(string number)
        {
            Console.WriteLine(Convert.ToInt64(number, 16));
        }
    }
}
