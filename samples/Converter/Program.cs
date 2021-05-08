using System;

namespace Converter
{
    [App]
    public class Program
    {
        public class To 
        {
            public static void Bin(long number)
            {
                Console.WriteLine(Convert.ToString(number, 2));
            }
            public static void Octo(long number)
            {
                Console.WriteLine(Convert.ToString(number, 8));
            }
            public static void Hex(long number)
            {
                Console.WriteLine(Convert.ToString(number, 16));
            }
        }
        public class From 
        {
            public static void Bin(string number)
            {
                Console.WriteLine(Convert.ToInt64(number, 2));
            }
            public static void Octo(string number)
            {
                Console.WriteLine(Convert.ToInt64(number, 8));
            }
            public static void Hex(string number)
            {
                Console.WriteLine(Convert.ToInt64(number, 16));
            }
        }
    }
}
