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

        public void WriteError(Exception ex)
        {
            this.WriteObject(new Error(ex));
        }

        public void WriteModelStart(string type, int propertyCount)
        {
            this.WriteString("model");
            this.WriteObject(new ModelHeader() { PropertyCount = propertyCount });
        }
        public void WriteModelEnd()
        {
            this.WriteString("/model");
        }

        public void WriteModelPropertyStart(string name, string type, bool isNull)
        {
            this.WriteString("prop");
            this.WriteObject(new PropertyHeader() { Name = name, IsNull = isNull });
        }
        public void WriteModelPropertyEnd()
        {
            this.WriteString("/prop");
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
            this.WriteString("list");
            this.WriteObject(new ListHeader() { ItemCount = itemCount });
        }
        public void WriteListEnd()
        {
            this.WriteString("/list");
        }

        public IError ReadError()
        {
            return this.ReadObject(Error.Serializer);
        }

        public IModelHeader ReadModelStart()
        {
            string data = this.ReadString();

            if (data != "model")
            {
                throw new Exception("Invalid message");
            }

            return this.ReadObject(ModelHeader.Serializer);
        }
        public void ReadModelEnd()
        {
            string data = this.ReadString();

            if (data != "/model")
            {
                throw new Exception("Invalid message");
            }
        }

        public IPropertyHeader ReadModelPropertyStart()
        {
            string data = this.ReadString();

            if (data != "prop")
            {
                throw new Exception("Invalid message");
            }

            return this.ReadObject(PropertyHeader.Serializer);
        }
        public void ReadModelPropertyEnd()
        {
            string data = this.ReadString();

            if (data != "/prop")
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
            string data = this.ReadString();

            if (data != "list")
            {
                throw new Exception("Invalid message");
            }

            return this.ReadObject(ListHeader.Serializer);
        }
        public void ReadListEnd()
        {
            string data = this.ReadString();

            if (data != "/list")
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

            this.WriteString("res");
            this.WriteObject(new ResponseHeader() { IsValid = success });
        }
        public void WriteResponseEnd()
        {
            this.WriteString("/res");

            this.EndWrite();
        }

        public IRequestHeader ReadRequestStart()
        {
            this.BeginRead();

            string data = this.ReadString();

            if (data != "req")
            {
                throw new Exception("Invalid message");
            }

            return this.ReadObject(RequestHeader.Serializer);
        }
        public void ReadRequestEnd()
        {
            string data = this.ReadString();

            if (data != "/req")
            {
                throw new Exception("Invalid message");
            }

            this.EndRead();
        }

        public IArgumentHeader ReadRequestArgumentStart()
        {
            string data = this.ReadString();

            if (data != "arg")
            {
                throw new Exception("Invalid message");
            }

            return this.ReadObject(ArgumentHeader.Serializer);
        }
        public void ReadRequestArgumentEnd()
        {
            string data = this.ReadString();

            if (data != "/arg")
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

            this.WriteString("req");
            this.WriteObject(new RequestHeader() { Method = methodName });
        }
        public void WriteRequestEnd()
        {
            this.WriteString("/req");

            this.EndWrite();
        }

        public void WriteRequestArgumentStart(string name, string type)
        {
            this.WriteString("arg");
            this.WriteObject(new ArgumentHeader() { Name = name });
        }
        public void WriteRequestArgumentEnd()
        {
            this.WriteString("/arg");
        }

        public IResponseHeader ReadResponseStart()
        {
            this.BeginRead();

            string data = this.ReadString();

            if (data != "res")
            {
                throw new Exception("Invalid message");
            }

            return this.ReadObject(ResponseHeader.Serializer);
        }
        public void ReadResponseEnd()
        {
            string data = this.ReadString();

            if (data != "/res")
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
