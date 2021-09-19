using System;
namespace NAMESPACE 
{
    public interface IArgumentConverter<T> 
    {
        T Convert(string str);
    }
}