using System;
namespace NAMESPACE 
{ 
    public class ArgumentParsingException : Exception
    {
        public ArgumentParsingException(string msg) : base(msg){}
    }
}