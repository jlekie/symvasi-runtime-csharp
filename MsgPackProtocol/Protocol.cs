using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using MessagePack = MsgPack;
using MsgPack.Serialization;

using Symvasi.Runtime.Transport;

//using MsgPack;
//using MsgPack.Compiler;

namespace Symvasi.Runtime.Protocol.MsgPack
{
    public enum HeaderCodes : byte
    {
        ReqStart = 0,
        ReqEnd = 1,

        ResStart = 2,
        ResEnd = 3,

        ReqArgStart = 4,
        ReqArgEnd = 5,

        ModelStart = 6,
        ModelEnd = 7,

        PropStart = 8,
        PropEnd = 9,

        ListStart = 10,
        ListEnd = 11,

        IndefinateStart = 12,
        IndefinateEnd = 13
    }

    public abstract class AMsgPackProtocol : IProtocol<ITransport>
    {
        public ITransport Transport { get; private set; }

        protected MemoryStream WriteStream { get; private set; }
        protected MessagePack.Packer WritePacker { get; private set; }

        protected MemoryStream ReadStream { get; private set; }
        protected MessagePack.Unpacker ReadUnpacker { get; private set; }

        public AMsgPackProtocol(ITransport transport)
        {
            this.Transport = transport;
        }

        protected void BeginWrite()
        {
            this.WriteStream = new MemoryStream();
            this.WritePacker = MessagePack.Packer.Create(this.WriteStream);
        }
        protected void EndWrite()
        {
            this.WriteStream.Position = 0;

            var data = this.WriteStream.ToArray();
            this.Send(data);

            this.WritePacker.Dispose();
            this.WritePacker = null;

            this.WriteStream.Dispose();
            this.WriteStream = null;
        }

        protected void BeginRead()
        {
            var data = this.Receive();

            this.ReadStream = new MemoryStream(data);
            this.ReadUnpacker = MessagePack.Unpacker.Create(this.ReadStream);
        }
        protected void EndRead()
        {
            this.ReadUnpacker.Dispose();
            this.ReadUnpacker = null;

            this.ReadStream.Dispose();
            this.ReadStream = null;
        }

        protected void WriteHeaderCode(HeaderCodes headerCode)
        {
            this.WritePacker.Pack((byte)headerCode);
        }
        protected void WriteString(string data)
        {
            this.WritePacker.PackString(data);
        }
        protected void WriteBoolean(bool data)
        {
            this.WritePacker.Pack(data);
        }
        protected void WriteInteger(int data)
        {
            this.WritePacker.Pack(data);
        }
        protected void WriteFloat(float data)
        {
            this.WritePacker.Pack(data);
        }
        protected void WriteDouble(double data)
        {
            this.WritePacker.Pack(data);
        }
        protected void WriteByte(byte data)
        {
            this.WritePacker.Pack(data);
        }
        protected void WriteEnum<T>(T data) where T : struct, IConvertible
        {
            this.WritePacker.Pack(data.ToInt32(System.Globalization.CultureInfo.InvariantCulture));
        }
        protected void WriteObject<T>(T data) where T : IMsgPackSerializable<T>
        {
            data.GetSerializer().Pack(this.WriteStream, data);
        }
        protected void WriteHeader<T>(T data) where T : IHeader
        {
            data.Write(this);
        }

        protected HeaderCodes ReadHeaderCode()
        {
            byte code;
            if (!this.ReadUnpacker.ReadByte(out code))
            {
                throw new Exception("Invalid data");
            }

            return (HeaderCodes)code;
        }
        protected string ReadString()
        {
            string data;
            if (!this.ReadUnpacker.ReadString(out data))
            {
                throw new Exception("Invalid data");
            }

            return data;
        }
        protected bool ReadBoolean()
        {
            bool data;
            if (!this.ReadUnpacker.ReadBoolean(out data))
            {
                throw new Exception("Invalid data");
            }

            return data;
        }
        protected int ReadInteger()
        {
            int data;
            if (!this.ReadUnpacker.ReadInt32(out data))
            {
                throw new Exception("Invalid data");
            }

            return data;
        }
        protected float ReadFloat()
        {
            float data;
            if (!this.ReadUnpacker.ReadSingle(out data))
            {
                throw new Exception("Invalid data");
            }

            return data;
        }
        protected double ReadDouble()
        {
            double data;
            if (!this.ReadUnpacker.ReadDouble(out data))
            {
                throw new Exception("Invalid data");
            }

            return data;
        }
        protected byte ReadByte()
        {
            byte data;
            if (!this.ReadUnpacker.ReadByte(out data))
            {
                throw new Exception("Invalid data");
            }

            return data;
        }
        protected T ReadEnum<T>() where T : struct, IConvertible
        {
            int data;
            if (!this.ReadUnpacker.ReadInt32(out data))
            {
                throw new Exception("Invalid data");
            }

            T parsedEnum;
            try
            {
                parsedEnum = (T)Enum.ToObject(typeof(T), data);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid data", ex);
            }

            return parsedEnum;
        }
        protected T ReadObject<T>(MessagePackSerializer<T> serializer)
        {
            try
            {
                return serializer.Unpack(this.ReadStream);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid data", ex);
            }
        }
        protected T ReadHeader<T>() where T : IHeader, new()
        {
            var result = new T();
            result.Read(this);

            return result;
        }

        public void WriteError(Exception ex)
        {
            this.WriteObject(new Error(ex));
        }

        public void WriteModelStart(string type, int propertyCount)
        {
            this.WriteHeaderCode(HeaderCodes.ModelStart);
            this.WriteHeader(new ModelHeader() { PropertyCount = propertyCount });
        }
        public void WriteModelEnd()
        {
            this.WriteHeaderCode(HeaderCodes.ModelEnd);
        }

        public void WriteModelPropertyStart(string name, string type, bool isNull)
        {
            this.WriteHeaderCode(HeaderCodes.PropStart);
            this.WriteHeader(new PropertyHeader() { Name = name, IsNull = isNull });
        }
        public void WriteModelPropertyEnd()
        {
            this.WriteHeaderCode(HeaderCodes.PropEnd);
        }

        public void WriteStringValue(string value)
        {
            this.WriteString(value);
        }
        public void WriteBoolValue(bool value)
        {
            this.WriteBoolean(value);
        }
        public void WriteIntegerValue(int value)
        {
            this.WriteInteger(value);
        }
        public void WriteFloatValue(float value)
        {
            this.WriteFloat(value);
        }
        public void WriteDoubleValue(double value)
        {
            this.WriteDouble(value);
        }
        public void WriteByteValue(byte value)
        {
            this.WriteByte(value);
        }
        public void WriteEnumValue<T>(T value) where T : struct, IConvertible
        {
            this.WriteEnum(value);
        }

        public void WriteListStart(int itemCount)
        {
            this.WriteHeaderCode(HeaderCodes.ListStart);
            this.WriteHeader(new ListHeader() { ItemCount = itemCount });
        }
        public void WriteListEnd()
        {
            this.WriteHeaderCode(HeaderCodes.ListEnd);
        }

        public void WriteIndefinateStart(IndefinateTypes type, string declaredType = null)
        {
            this.WriteHeaderCode(HeaderCodes.IndefinateStart);
            this.WriteHeader(new IndefinateHeader() { Type = type, DeclaredType = declaredType });
        }
        public void WriteIndefinateEnd()
        {
            this.WriteHeaderCode(HeaderCodes.IndefinateEnd);
        }

        public IError ReadError()
        {
            return this.ReadObject(Error.Serializer);
        }

        public IModelHeader ReadModelStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ModelStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<ModelHeader>();
        }
        public void ReadModelEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ModelEnd)
            {
                throw new Exception("Invalid message");
            }
        }

        public IPropertyHeader ReadModelPropertyStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.PropStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<PropertyHeader>();
        }
        public void ReadModelPropertyEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.PropEnd)
            {
                throw new Exception("Invalid message");
            }
        }

        public string ReadStringValue()
        {
            return this.ReadString();
        }
        public bool ReadBoolValue()
        {
            return this.ReadBoolean();
        }
        public int ReadIntegerValue()
        {
            return this.ReadInteger();
        }
        public float ReadFloatValue()
        {
            return this.ReadFloat();
        }
        public double ReadDoubleValue()
        {
            return this.ReadDouble();
        }
        public byte ReadByteValue()
        {
            return this.ReadByte();
        }
        public T ReadEnumValue<T>() where T : struct, IConvertible
        {
            return this.ReadEnum<T>();
        }

        public IListHeader ReadListStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ListStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<ListHeader>();
        }
        public void ReadListEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ListEnd)
            {
                throw new Exception("Invalid message");
            }
        }

        public IIndefinateHeader ReadIndefinateStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.IndefinateStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<IndefinateHeader>();
        }
        public void ReadIndefinateEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.IndefinateEnd)
            {
                throw new Exception("Invalid message");
            }
        }

        protected abstract void Send(byte[] data);
        protected abstract byte[] Receive();
    }

    public class MsgPackServerProtocol : AMsgPackProtocol, IServerProtocol
    {
        public Guid? SchedulerId { get; private set; }

        public MsgPackServerProtocol(IServerTransport transport)
            : base(transport)
        {
        }
        public MsgPackServerProtocol(IServerTransport transport, Guid scheduler)
            : this(transport)
        {
            this.SchedulerId = scheduler;
        }

        public void WriteResponseStart(bool success)
        {
            this.BeginWrite();

            this.WriteHeaderCode(HeaderCodes.ResStart);
            this.WriteHeader(new ResponseHeader() { IsValid = success });
        }
        public void WriteResponseEnd()
        {
            this.WriteHeaderCode(HeaderCodes.ResEnd);

            this.EndWrite();
        }

        public IRequestHeader ReadRequestStart()
        {
            this.BeginRead();

            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ReqStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<RequestHeader>();
        }
        public void ReadRequestEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ReqEnd)
            {
                throw new Exception("Invalid message");
            }

            this.EndRead();
        }

        public IArgumentHeader ReadRequestArgumentStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ReqArgStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<ArgumentHeader>();
        }
        public void ReadRequestArgumentEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ReqArgEnd)
            {
                throw new Exception("Invalid message");
            }
        }

        public new IServerTransport Transport
        {
            get
            {
                return (IServerTransport)base.Transport;
            }
        }

        protected override void Send(byte[] data)
        {
            if (this.SchedulerId.HasValue)
            {
                this.Transport.Send(data, this.SchedulerId.Value);
            }
            else
            {
                this.Transport.Send(data);
            }
        }
        protected override byte[] Receive()
        {
            if (this.SchedulerId.HasValue)
            {
                return this.Transport.Receive(this.SchedulerId.Value);
            }
            else
            {
                return this.Transport.Receive();
            }
        }
    }

    public class MsgPackClientProtocol : AMsgPackProtocol, IClientProtocol
    {
        public MsgPackClientProtocol(IClientTransport transport)
            : base(transport)
        {
        }

        public void WriteRequestStart(string methodName)
        {
            this.BeginWrite();

            this.WriteHeaderCode(HeaderCodes.ReqStart);
            this.WriteHeader(new RequestHeader() { Method = methodName });
        }
        public void WriteRequestEnd()
        {
            this.WriteHeaderCode(HeaderCodes.ReqEnd);

            this.EndWrite();
        }

        public void WriteRequestArgumentStart(string name, string type)
        {
            this.WriteHeaderCode(HeaderCodes.ReqArgStart);
            this.WriteHeader(new ArgumentHeader() { Name = name });
        }
        public void WriteRequestArgumentEnd()
        {
            this.WriteHeaderCode(HeaderCodes.ReqArgEnd);
        }

        public IResponseHeader ReadResponseStart()
        {
            this.BeginRead();

            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ResStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<ResponseHeader>();
        }
        public void ReadResponseEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ResEnd)
            {
                throw new Exception("Invalid message");
            }

            this.EndRead();
        }

        public new IClientTransport Transport
        {
            get
            {
                return (IClientTransport)base.Transport;
            }
        }

        protected override void Send(byte[] data)
        {
            this.Transport.Send(data);
        }
        protected override byte[] Receive()
        {
            return this.Transport.Receive();
        }
    }
}
