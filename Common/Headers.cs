using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symvasi.Runtime.Protocol
{
    public interface IArgumentHeader
    {
        string Name { get; }
    }
    public interface IListHeader
    {
        int ItemCount { get; }
    }
    public interface IModelHeader
    {
        int PropertyCount { get; }
    }
    public interface IPropertyHeader
    {
        string Name { get; }
        bool IsNull { get; }
    }
    public interface IRequestHeader
    {
        string Method { get; }
        int ArgumentCount { get; }
    }
    public interface IResponseHeader
    {
        bool IsValid { get; }
    }
    public interface IError
    {
        string Message { get; }
        Exception CreateException();
    }
}
