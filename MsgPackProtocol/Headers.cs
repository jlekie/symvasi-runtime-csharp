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

    [DataContract]
    public class RequestHeader : IRequestHeader, IMsgPackSerializable<RequestHeader>
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
    }
    [DataContract]
    public class ResponseHeader : IResponseHeader, IMsgPackSerializable<ResponseHeader>
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
    }
    [DataContract]
    public class ArgumentHeader : IArgumentHeader, IMsgPackSerializable<ArgumentHeader>
    {
        public static readonly MessagePackSerializer<ArgumentHeader> Serializer = MessagePackSerializer.Get<ArgumentHeader>(new SerializationContext() { SerializationMethod = SerializationMethod.Map });

        [DataMember(Name = "name")]
        public string Name { get; set; }

        public MessagePackSerializer<ArgumentHeader> GetSerializer()
        {
            return ArgumentHeader.Serializer;
        }
    }
    [DataContract]
    public class ModelHeader : IModelHeader, IMsgPackSerializable<ModelHeader>
    {
        public static readonly MessagePackSerializer<ModelHeader> Serializer = MessagePackSerializer.Get<ModelHeader>(new SerializationContext() { SerializationMethod = SerializationMethod.Map });

        [DataMember(Name = "propertyCount")]
        public int PropertyCount { get; set; }

        public MessagePackSerializer<ModelHeader> GetSerializer()
        {
            return ModelHeader.Serializer;
        }
    }
    [DataContract]
    public class PropertyHeader : IPropertyHeader, IMsgPackSerializable<PropertyHeader>
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
    }
    [DataContract]
    public class ListHeader : IListHeader, IMsgPackSerializable<ListHeader>
    {
        public static readonly MessagePackSerializer<ListHeader> Serializer = MessagePackSerializer.Get<ListHeader>(new SerializationContext() { SerializationMethod = SerializationMethod.Map });

        [DataMember(Name = "itemCount")]
        public int ItemCount { get; set; }

        public MessagePackSerializer<ListHeader> GetSerializer()
        {
            return ListHeader.Serializer;
        }
    }
    [DataContract]
    public class Error : IError, IMsgPackSerializable<Error>
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
    }
}
