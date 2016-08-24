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
    public interface IMsgPackProtocol : IProtocol
    {
    }

    public class MsgPackProtocol : AProtocol, IMsgPackProtocol
    {
        private MemoryStream WriteStream { get; set; }
        private MessagePack.Packer WritePacker { get; set; }

        private MemoryStream ReadStream { get; set; }
        private MessagePack.Unpacker ReadUnpacker { get; set; }

        public MsgPackProtocol(ITransport transport)
            : base(transport)
        {
        }

        protected async Task BeginRead()
        {
            var data = await this.Transport.Receive();

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

        protected void BeginWrite()
        {
            this.WriteStream = new MemoryStream();
            this.WritePacker = MessagePack.Packer.Create(this.WriteStream);
        }
        protected async Task EndWrite()
        {
            this.WriteStream.Position = 0;

            var data = this.WriteStream.ToArray();
            await this.Transport.Send(data);

            this.WritePacker.Dispose();
            this.WritePacker = null;

            this.WriteStream.Dispose();
            this.WriteStream = null;
        }

        public override async Task<T> ReadData<T>(Func<T> reader)
        {
            await this.BeginRead();
            var result = reader();
            this.EndRead();

            return result;
        }
        public override async Task WriteData(Action writer)
        {
            this.BeginWrite();
            writer();
            await this.EndWrite();
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
        protected void WriteHeaderCode(HeaderCodes headerCode)
        {
            this.WritePacker.Pack((byte)headerCode);
        }
        protected void WriteHeader<T>(T data) where T : IHeader
        {
            data.Write(this);
        }

        protected string ReadString()
        {
            return this.ReadUnpacker.ReadItemData().AsString();
        }
        protected bool ReadBoolean()
        {
            return this.ReadUnpacker.ReadItemData().AsBoolean();
        }
        protected int ReadInteger()
        {
            return this.ReadUnpacker.ReadItemData().AsInt32();
        }
        protected float ReadFloat()
        {
            return this.ReadUnpacker.ReadItemData().AsSingle();
        }
        protected double ReadDouble()
        {
            return this.ReadUnpacker.ReadItemData().AsDouble();
        }
        protected byte ReadByte()
        {
            return this.ReadUnpacker.ReadItemData().AsByte();
        }
        protected T ReadEnum<T>() where T : struct, IConvertible
        {
            int data = this.ReadUnpacker.ReadItemData().AsInt32();

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
        protected HeaderCodes ReadHeaderCode()
        {
            byte code = this.ReadUnpacker.ReadItemData().AsByte();

            return (HeaderCodes)code;
        }
        protected T ReadHeader<T>() where T : IHeader, new()
        {
            var result = new T();
            result.Read(this);

            return result;
        }

        public override void WriteError(Exception ex)
        {
            this.WriteHeader(new ErrorHeader(ex));
        }

        public override void WriteModelStart(int propertyCount)
        {
            this.WriteHeaderCode(HeaderCodes.ModelStart);
            this.WriteHeader(new ModelHeader(propertyCount));
        }
        public override void WriteAbstractModelStart(int propertyCount, string derivedModelName)
        {
            this.WriteHeaderCode(HeaderCodes.ModelStart);
            this.WriteHeader(new AbstractModelHeader(propertyCount, derivedModelName));
        }
        public override void WriteModelEnd()
        {
            this.WriteHeaderCode(HeaderCodes.ModelEnd);
        }

        public override void WriteModelPropertyStart(string name, string type, bool isNull)
        {
            this.WriteHeaderCode(HeaderCodes.PropStart);
            this.WriteHeader(new PropertyHeader(name, isNull));
        }
        public override void WriteModelPropertyEnd()
        {
            this.WriteHeaderCode(HeaderCodes.PropEnd);
        }

        public override void WriteStringValue(string value)
        {
            this.WriteString(value);
        }
        public override void WriteBoolValue(bool value)
        {
            this.WriteBoolean(value);
        }
        public override void WriteIntegerValue(int value)
        {
            this.WriteInteger(value);
        }
        public override void WriteFloatValue(float value)
        {
            this.WriteFloat(value);
        }
        public override void WriteDoubleValue(double value)
        {
            this.WriteDouble(value);
        }
        public override void WriteByteValue(byte value)
        {
            this.WriteByte(value);
        }
        public override void WriteEnumValue<T>(T value)
        {
            this.WriteEnum(value);
        }

        public override void WriteListStart(int itemCount)
        {
            this.WriteHeaderCode(HeaderCodes.ListStart);
            this.WriteHeader(new ListHeader(itemCount));
        }
        public override void WriteListEnd()
        {
            this.WriteHeaderCode(HeaderCodes.ListEnd);
        }

        public override void WriteMapStart(int itemCount)
        {
            this.WriteHeaderCode(HeaderCodes.MapStart);
            this.WriteHeader(new MapHeader(itemCount));
        }
        public override void WriteMapEnd()
        {
            this.WriteHeaderCode(HeaderCodes.MapEnd);
        }

        public override void WriteIndefinateStart(IndefinateTypes type, string declaredType = null)
        {
            this.WriteHeaderCode(HeaderCodes.IndefinateStart);
            this.WriteHeader(new IndefinateHeader(type, declaredType));
        }
        public override void WriteIndefinateEnd()
        {
            this.WriteHeaderCode(HeaderCodes.IndefinateEnd);
        }

        public override IErrorHeader ReadError()
        {
            return this.ReadHeader<ErrorHeader>();
        }

        public override IModelHeader ReadModelStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ModelStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<ModelHeader>();
        }
        public override IAbstractModelHeader ReadAbstractModelStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ModelStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<AbstractModelHeader>();
        }
        public override void ReadModelEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ModelEnd)
            {
                throw new Exception("Invalid message");
            }
        }

        public override IPropertyHeader ReadModelPropertyStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.PropStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<PropertyHeader>();
        }
        public override void ReadModelPropertyEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.PropEnd)
            {
                throw new Exception("Invalid message");
            }
        }

        public override string ReadStringValue()
        {
            return this.ReadString();
        }
        public override bool ReadBoolValue()
        {
            return this.ReadBoolean();
        }
        public override int ReadIntegerValue()
        {
            return this.ReadInteger();
        }
        public override float ReadFloatValue()
        {
            return this.ReadFloat();
        }
        public override double ReadDoubleValue()
        {
            return this.ReadDouble();
        }
        public override byte ReadByteValue()
        {
            return this.ReadByte();
        }
        public override T ReadEnumValue<T>()
        {
            return this.ReadEnum<T>();
        }

        public override IListHeader ReadListStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ListStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<ListHeader>();
        }
        public override void ReadListEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ListEnd)
            {
                throw new Exception("Invalid message");
            }
        }

        public override IMapHeader ReadMapStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.MapStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<MapHeader>();
        }
        public override void ReadMapEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.MapEnd)
            {
                throw new Exception("Invalid message");
            }
        }

        public override IIndefinateHeader ReadIndefinateStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.IndefinateStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<IndefinateHeader>();
        }
        public override void ReadIndefinateEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.IndefinateEnd)
            {
                throw new Exception("Invalid message");
            }
        }

        public override void WriteResponseStart(bool success)
        {
            this.WriteHeaderCode(HeaderCodes.ResStart);
            this.WriteHeader(new ResponseHeader(success));
        }
        public override void WriteResponseEnd()
        {
            this.WriteHeaderCode(HeaderCodes.ResEnd);
        }

        public override IRequestHeader ReadRequestStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ReqStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<RequestHeader>();
        }
        public override void ReadRequestEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ReqEnd)
            {
                throw new Exception("Invalid message");
            }
        }

        public override IArgumentHeader ReadRequestArgumentStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ReqArgStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<ArgumentHeader>();
        }
        public override void ReadRequestArgumentEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ReqArgEnd)
            {
                throw new Exception("Invalid message");
            }
        }

        public override void WriteRequestStart(string methodName, int argumentCount, IDictionary<string, string> tags = null)
        {
            this.WriteHeaderCode(HeaderCodes.ReqStart);
            this.WriteHeader(new RequestHeader(methodName, argumentCount, tags));
        }
        public override void WriteRequestEnd()
        {
            this.WriteHeaderCode(HeaderCodes.ReqEnd);
        }

        public override void WriteRequestArgumentStart(string name, string type)
        {
            this.WriteHeaderCode(HeaderCodes.ReqArgStart);
            this.WriteHeader(new ArgumentHeader(name));
        }
        public override void WriteRequestArgumentEnd()
        {
            this.WriteHeaderCode(HeaderCodes.ReqArgEnd);
        }

        public override IResponseHeader ReadResponseStart()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ResStart)
            {
                throw new Exception("Invalid message");
            }

            return this.ReadHeader<ResponseHeader>();
        }
        public override void ReadResponseEnd()
        {
            var header = this.ReadHeaderCode();
            if (header != HeaderCodes.ResEnd)
            {
                throw new Exception("Invalid message");
            }
        }
    }

    //public class MsgPackServerProtocol : AMsgPackProtocol, IServerProtocol
    //{
    //    public Guid? SchedulerId { get; private set; }

    //    public MsgPackServerProtocol(IServerTransport transport)
    //        : base(transport)
    //    {
    //    }
    //    public MsgPackServerProtocol(IServerTransport transport, Guid scheduler)
    //        : this(transport)
    //    {
    //        this.SchedulerId = scheduler;
    //    }

    //    public void WriteResponseStart(bool success)
    //    {
    //        this.BeginWrite();

    //        this.WriteHeaderCode(HeaderCodes.ResStart);
    //        this.WriteHeader(new ResponseHeader() { IsValid = success });
    //    }
    //    public void WriteResponseEnd()
    //    {
    //        this.WriteHeaderCode(HeaderCodes.ResEnd);

    //        this.EndWrite();
    //    }

    //    public IRequestHeader ReadRequestStart()
    //    {
    //        this.BeginRead();

    //        var header = this.ReadHeaderCode();
    //        if (header != HeaderCodes.ReqStart)
    //        {
    //            throw new Exception("Invalid message");
    //        }

    //        return this.ReadHeader<RequestHeader>();
    //    }
    //    public void ReadRequestEnd()
    //    {
    //        var header = this.ReadHeaderCode();
    //        if (header != HeaderCodes.ReqEnd)
    //        {
    //            throw new Exception("Invalid message");
    //        }

    //        this.EndRead();
    //    }

    //    public IArgumentHeader ReadRequestArgumentStart()
    //    {
    //        var header = this.ReadHeaderCode();
    //        if (header != HeaderCodes.ReqArgStart)
    //        {
    //            throw new Exception("Invalid message");
    //        }

    //        return this.ReadHeader<ArgumentHeader>();
    //    }
    //    public void ReadRequestArgumentEnd()
    //    {
    //        var header = this.ReadHeaderCode();
    //        if (header != HeaderCodes.ReqArgEnd)
    //        {
    //            throw new Exception("Invalid message");
    //        }
    //    }

    //    public new IServerTransport Transport
    //    {
    //        get
    //        {
    //            return (IServerTransport)base.Transport;
    //        }
    //    }

    //    protected override void Send(byte[] data)
    //    {
    //        if (this.SchedulerId.HasValue)
    //        {
    //            this.Transport.Send(data, this.SchedulerId.Value);
    //        }
    //        else
    //        {
    //            this.Transport.Send(data);
    //        }
    //    }
    //    protected override byte[] Receive()
    //    {
    //        if (this.SchedulerId.HasValue)
    //        {
    //            return this.Transport.Receive(this.SchedulerId.Value);
    //        }
    //        else
    //        {
    //            return this.Transport.Receive();
    //        }
    //    }
    //}

    //public class MsgPackClientProtocol : AMsgPackProtocol, IClientProtocol
    //{
    //    public MsgPackClientProtocol(IClientTransport transport)
    //        : base(transport)
    //    {
    //    }

    //    public void WriteRequestStart(string methodName, int argumentCount, IDictionary<string, string> tags = null)
    //    {
    //        this.BeginWrite();

    //        this.WriteHeaderCode(HeaderCodes.ReqStart);

    //        var header = new RequestHeader() { Method = methodName, ArgumentCount = argumentCount };
    //        if (tags != null)
    //        {
    //            foreach (var tag in tags)
    //                header.Tags.Add(tag.Key, tag.Value);
    //        }
    //        this.WriteHeader(header);
    //    }
    //    public void WriteRequestEnd()
    //    {
    //        this.WriteHeaderCode(HeaderCodes.ReqEnd);

    //        this.EndWrite();
    //    }

    //    public void WriteRequestArgumentStart(string name, string type)
    //    {
    //        this.WriteHeaderCode(HeaderCodes.ReqArgStart);
    //        this.WriteHeader(new ArgumentHeader() { Name = name });
    //    }
    //    public void WriteRequestArgumentEnd()
    //    {
    //        this.WriteHeaderCode(HeaderCodes.ReqArgEnd);
    //    }

    //    public IResponseHeader ReadResponseStart()
    //    {
    //        this.BeginRead();

    //        var header = this.ReadHeaderCode();
    //        if (header != HeaderCodes.ResStart)
    //        {
    //            throw new Exception("Invalid message");
    //        }

    //        return this.ReadHeader<ResponseHeader>();
    //    }
    //    public void ReadResponseEnd()
    //    {
    //        var header = this.ReadHeaderCode();
    //        if (header != HeaderCodes.ResEnd)
    //        {
    //            throw new Exception("Invalid message");
    //        }

    //        this.EndRead();
    //    }

    //    public new IClientTransport Transport
    //    {
    //        get
    //        {
    //            return (IClientTransport)base.Transport;
    //        }
    //    }

    //    protected override void Send(byte[] data)
    //    {
    //        this.Transport.Send(data);
    //    }
    //    protected override byte[] Receive()
    //    {
    //        return this.Transport.Receive();
    //    }
    //}
}
