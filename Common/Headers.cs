using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symvasi.Runtime.Protocol
{
    public enum IndefinateTypes
    {
        String = 0,
        Boolean = 1,
        Integer = 2,
        Float = 3,
        Double = 4,
        Byte = 5,
        Enum = 6,
        Model = 7,
        List = 8
    }

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
    public interface IIndefinateHeader
    {
        IndefinateTypes Type { get; }
        string DeclaredType { get; }
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
