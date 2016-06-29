using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Symvasi.Runtime.Transport;

namespace Symvasi.Runtime.Protocol
{
    public interface IProtocol
    {
        #region Write Operations

        void WriteError(Exception ex);

        void WriteModelStart(string type, int propertyCount);
        void WriteModelEnd();

        void WriteModelPropertyStart(string name, string type, bool isNull);
        void WriteModelPropertyEnd();

        void WriteStringValue(string value);
        void WriteBoolValue(bool value);
        void WriteIntegerValue(int value);
        void WriteFloatValue(float value);
        void WriteDoubleValue(double value);
        void WriteByteValue(byte value);
        void WriteEnumValue<T>(T value) where T : struct, IConvertible;

        void WriteListStart(int itemCount);
        void WriteListEnd();

        #endregion

        #region Read Operations

        IError ReadError();

        IModelHeader ReadModelStart();
        void ReadModelEnd();

        IPropertyHeader ReadModelPropertyStart();
        void ReadModelPropertyEnd();

        string ReadStringValue();
        bool ReadBoolValue();
        int ReadIntegerValue();
        float ReadFloatValue();
        double ReadDoubleValue();
        byte ReadByteValue();
        T ReadEnumValue<T>() where T : struct, IConvertible;

        IListHeader ReadListStart();
        void ReadListEnd();

        #endregion
    }
    public interface IProtocol<T> : IProtocol where T : ITransport
    {
        T Transport { get; }
    }
    public interface IServerProtocol : IProtocol<IServerTransport>
    {
        #region Write Operations

        void WriteResponseStart(bool success);
        void WriteResponseEnd();

        #endregion

        #region Read Operations

        IRequestHeader ReadRequestStart();
        void ReadRequestEnd();

        IArgumentHeader ReadRequestArgumentStart();
        void ReadRequestArgumentEnd();

        #endregion
    }
    public interface IClientProtocol : IProtocol<IClientTransport>
    {
        #region Write Operations

        void WriteRequestStart(string methodName);
        void WriteRequestEnd();

        void WriteRequestArgumentStart(string name, string type);
        void WriteRequestArgumentEnd();

        #endregion

        #region Read Operations

        IResponseHeader ReadResponseStart();
        void ReadResponseEnd();

        #endregion
    }
}
