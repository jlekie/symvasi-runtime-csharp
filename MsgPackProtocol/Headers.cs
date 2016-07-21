using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

using MsgPack;
using MsgPack.Serialization;

namespace Symvasi.Runtime.Protocol.MsgPack
{
    public interface IMsgPackSerializable<T>
    {
        MessagePackSerializer<T> GetSerializer();
    }
    public interface IHeader
    {
        void Read(IProtocol protocol);
        void Write(IProtocol protocol);
    }

    [DataContract]
    public class RequestHeader :  IRequestHeader, IHeader, IMsgPackSerializable<RequestHeader>
    {
        public static readonly MessagePackSerializer<RequestHeader> Serializer = MessagePackSerializer.Get<RequestHeader>(new SerializationContext() { SerializationMethod = SerializationMethod.Map });

        [DataMember(Name = "method")]
        public string Method { get; set; }
        [DataMember(Name = "argumentCount")]
        public int ArgumentCount { get; set; }

        public MessagePackSerializer<RequestHeader> GetSerializer()
        {
            return RequestHeader.Serializer;
        }

        public void Read(IProtocol protocol)
        {
            this.Method = protocol.ReadStringValue();
            this.ArgumentCount = protocol.ReadIntegerValue();
        }
        public void Write(IProtocol protocol)
        {
            protocol.WriteStringValue(this.Method);
            protocol.WriteIntegerValue(this.ArgumentCount);
        }
    }
    [DataContract]
    public class ResponseHeader : IResponseHeader, IHeader, IMsgPackSerializable<ResponseHeader>
    {
        public static readonly MessagePackSerializer<ResponseHeader> Serializer = MessagePackSerializer.Get<ResponseHeader>(new SerializationContext() { SerializationMethod = SerializationMethod.Map });

        public ResponseHeader()
        {
            this.Test = "123abc";
        }

        [DataMember(Name = "isValid")]
        public bool IsValid { get; set; }

        [DataMember(Name = "test")]
        public string Test { get; set; }

        public MessagePackSerializer<ResponseHeader> GetSerializer()
        {
            return ResponseHeader.Serializer;
        }

        public void Read(IProtocol protocol)
        {
            this.IsValid = protocol.ReadBoolValue();
            this.Test = protocol.ReadStringValue();
        }
        public void Write(IProtocol protocol)
        {
            protocol.WriteBoolValue(this.IsValid);
            protocol.WriteStringValue(this.Test);
        }
    }
    [DataContract]
    public class ArgumentHeader : IArgumentHeader, IHeader, IMsgPackSerializable<ArgumentHeader>
    {
        public static readonly MessagePackSerializer<ArgumentHeader> Serializer = MessagePackSerializer.Get<ArgumentHeader>(new SerializationContext() { SerializationMethod = SerializationMethod.Map });

        [DataMember(Name = "name")]
        public string Name { get; set; }

        public MessagePackSerializer<ArgumentHeader> GetSerializer()
        {
            return ArgumentHeader.Serializer;
        }

        public void Read(IProtocol protocol)
        {
            this.Name = protocol.ReadStringValue();
        }
        public void Write(IProtocol protocol)
        {
            protocol.WriteStringValue(this.Name);
        }
    }
    [DataContract]
    public class ModelHeader : IModelHeader, IHeader, IMsgPackSerializable<ModelHeader>
    {
        public static readonly MessagePackSerializer<ModelHeader> Serializer = MessagePackSerializer.Get<ModelHeader>(new SerializationContext() { SerializationMethod = SerializationMethod.Map });

        [DataMember(Name = "propertyCount")]
        public int PropertyCount { get; set; }

        public MessagePackSerializer<ModelHeader> GetSerializer()
        {
            return ModelHeader.Serializer;
        }

        public void Read(IProtocol protocol)
        {
            this.PropertyCount = protocol.ReadIntegerValue();
        }
        public void Write(IProtocol protocol)
        {
            protocol.WriteIntegerValue(this.PropertyCount);
        }
    }
    [DataContract]
    public class PropertyHeader : IPropertyHeader, IHeader, IMsgPackSerializable<PropertyHeader>
    {
        public static readonly MessagePackSerializer<PropertyHeader> Serializer = MessagePackSerializer.Get<PropertyHeader>(new SerializationContext() { SerializationMethod = SerializationMethod.Map });

        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "isNull")]
        public bool IsNull { get; set; }

        public MessagePackSerializer<PropertyHeader> GetSerializer()
        {
            return PropertyHeader.Serializer;
        }

        public void Read(IProtocol protocol)
        {
            this.Name = protocol.ReadStringValue();
            this.IsNull = protocol.ReadBoolValue();
        }
        public void Write(IProtocol protocol)
        {
            protocol.WriteStringValue(this.Name);
            protocol.WriteBoolValue(this.IsNull);
        }
    }
    [DataContract]
    public class ListHeader : IListHeader, IHeader, IMsgPackSerializable<ListHeader>
    {
        public static readonly MessagePackSerializer<ListHeader> Serializer = MessagePackSerializer.Get<ListHeader>(new SerializationContext() { SerializationMethod = SerializationMethod.Map });

        [DataMember(Name = "itemCount")]
        public int ItemCount { get; set; }

        public MessagePackSerializer<ListHeader> GetSerializer()
        {
            return ListHeader.Serializer;
        }

        public void Read(IProtocol protocol)
        {
            this.ItemCount = protocol.ReadIntegerValue();
        }
        public void Write(IProtocol protocol)
        {
            protocol.WriteIntegerValue(this.ItemCount);
        }
    }
    [DataContract]
    public class IndefinateHeader : IIndefinateHeader, IHeader, IMsgPackSerializable<IndefinateHeader>
    {
        public static readonly MessagePackSerializer<IndefinateHeader> Serializer = MessagePackSerializer.Get<IndefinateHeader>(new SerializationContext() { SerializationMethod = SerializationMethod.Map });

        [DataMember(Name = "type")]
        public IndefinateTypes Type { get; set; }
        [DataMember(Name = "declaredType")]
        public string DeclaredType { get; set; }

        public MessagePackSerializer<IndefinateHeader> GetSerializer()
        {
            return IndefinateHeader.Serializer;
        }

        public void Read(IProtocol protocol)
        {
            this.Type = protocol.ReadEnumValue<IndefinateTypes>();

            switch (this.Type)
            {
                case IndefinateTypes.Enum:
                case IndefinateTypes.Model:
                    this.DeclaredType = protocol.ReadStringValue();
                    break;
            }
        }
        public void Write(IProtocol protocol)
        {
            protocol.WriteEnumValue(this.Type);

            switch (this.Type)
            {
                case IndefinateTypes.Enum:
                case IndefinateTypes.Model:
                    protocol.WriteStringValue(this.DeclaredType);
                    break;
            }
        }
    }
    [DataContract]
    public class Error : IError, IHeader, IMsgPackSerializable<Error>
    {
        public static readonly MessagePackSerializer<Error> Serializer = MessagePackSerializer.Get<Error>(new SerializationContext() { SerializationMethod = SerializationMethod.Map });

        [DataMember(Name = "message")]
        public string Message { get; set; }

        public Error()
        {
        }
        public Error(Exception ex)
        {
            this.Message = ex.Message;
        }

        public Exception CreateException()
        {
            return new Exception(this.Message);
        }

        public MessagePackSerializer<Error> GetSerializer()
        {
            return Error.Serializer;
        }

        public void Read(IProtocol protocol)
        {
            this.Message = protocol.ReadStringValue();
        }
        public void Write(IProtocol protocol)
        {
            protocol.WriteStringValue(this.Message);
        }
    }
}
