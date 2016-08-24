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
        Task<T> ReadData<T>(Func<T> reader);
        Task WriteData(Action writer);

        #region Write Operations

        void WriteError(Exception ex);

        void WriteModelStart(int propertyCount);
        void WriteAbstractModelStart(int propertyCount, string derivedModelName);
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

        void WriteMapStart(int itemCount);
        void WriteMapEnd();

        void WriteIndefinateStart(IndefinateTypes type, string declaredType = null);
        void WriteIndefinateEnd();

        void WriteResponseStart(bool success);
        void WriteResponseEnd();

        void WriteRequestStart(string methodName, int argumentCount, IDictionary<string, string> tags = null);
        void WriteRequestEnd();

        void WriteRequestArgumentStart(string name, string type);
        void WriteRequestArgumentEnd();

        #endregion

        #region Read Operations

        IErrorHeader ReadError();

        IModelHeader ReadModelStart();
        IAbstractModelHeader ReadAbstractModelStart();
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

        IMapHeader ReadMapStart();
        void ReadMapEnd();

        IIndefinateHeader ReadIndefinateStart();
        void ReadIndefinateEnd();

        IRequestHeader ReadRequestStart();
        void ReadRequestEnd();

        IArgumentHeader ReadRequestArgumentStart();
        void ReadRequestArgumentEnd();

        IResponseHeader ReadResponseStart();
        void ReadResponseEnd();

        #endregion
    }

    public abstract class AProtocol : IProtocol
    {
        public ITransport Transport { get; private set; }

        public AProtocol(ITransport transport)
        {
            this.Transport = transport;
        }

        public abstract Task<T> ReadData<T>(Func<T> reader);
        public abstract Task WriteData(Action writer);

        #region Write Operations

        public abstract void WriteStringValue(string value);
        public abstract void WriteBoolValue(bool value);
        public abstract void WriteIntegerValue(int value);
        public abstract void WriteFloatValue(float value);
        public abstract void WriteDoubleValue(double value);
        public abstract void WriteByteValue(byte value);
        public abstract void WriteEnumValue<T>(T value) where T : struct, IConvertible;

        public abstract void WriteModelStart(int propertyCount);
        public abstract void WriteAbstractModelStart(int propertyCount, string derivedModelName);
        public abstract void WriteModelEnd();

        public abstract void WriteModelPropertyStart(string name, string type, bool isNull);
        public abstract void WriteModelPropertyEnd();

        public abstract void WriteListStart(int itemCount);
        public abstract void WriteListEnd();

        public abstract void WriteMapStart(int itemCount);
        public abstract void WriteMapEnd();

        public abstract void WriteIndefinateStart(IndefinateTypes type, string declaredType = null);
        public abstract void WriteIndefinateEnd();

        public abstract void WriteRequestStart(string methodName, int argumentCount, IDictionary<string, string> tags = null);
        public abstract void WriteRequestEnd();

        public abstract void WriteRequestArgumentStart(string name, string type);
        public abstract void WriteRequestArgumentEnd();

        public abstract void WriteResponseStart(bool success);
        public abstract void WriteResponseEnd();

        public abstract void WriteError(Exception ex);

        #endregion

        #region Read Operations

        public abstract string ReadStringValue();
        public abstract bool ReadBoolValue();
        public abstract int ReadIntegerValue();
        public abstract float ReadFloatValue();
        public abstract double ReadDoubleValue();
        public abstract byte ReadByteValue();
        public abstract T ReadEnumValue<T>() where T : struct, IConvertible;

        public abstract IModelHeader ReadModelStart();
        public abstract IAbstractModelHeader ReadAbstractModelStart();
        public abstract void ReadModelEnd();

        public abstract IPropertyHeader ReadModelPropertyStart();
        public abstract void ReadModelPropertyEnd();

        public abstract IListHeader ReadListStart();
        public abstract void ReadListEnd();

        public abstract IMapHeader ReadMapStart();
        public abstract void ReadMapEnd();

        public abstract IIndefinateHeader ReadIndefinateStart();
        public abstract void ReadIndefinateEnd();

        public abstract IRequestHeader ReadRequestStart();
        public abstract void ReadRequestEnd();

        public abstract IArgumentHeader ReadRequestArgumentStart();
        public abstract void ReadRequestArgumentEnd();

        public abstract IResponseHeader ReadResponseStart();
        public abstract void ReadResponseEnd();

        public abstract IErrorHeader ReadError();

        #endregion
    }
}
