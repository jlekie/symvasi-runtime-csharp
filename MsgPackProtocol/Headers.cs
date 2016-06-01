using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MsgPack;
using MsgPack.Serialization;

namespace Symvasi.Runtime.Protocol.MsgPack
{
    public interface IMsgPackSerializable<T>
    {
        MessagePackSerializer<T> GetSerializer();
    }

    public class RequestHeader : IRequestHeader, IMsgPackSerializable<RequestHeader>
    {
        public static readonly MessagePackSerializer<RequestHeader> Serializer = MessagePackSerializer.Get<RequestHeader>();

        public string Method { get; set; }
        public int ArgumentCount { get; set; }

        public MessagePackSerializer<RequestHeader> GetSerializer()
        {
            return RequestHeader.Serializer;
        }
    }
    public class ResponseHeader : IResponseHeader, IMsgPackSerializable<ResponseHeader>
    {
        public static readonly MessagePackSerializer<ResponseHeader> Serializer = MessagePackSerializer.Get<ResponseHeader>();

        public bool IsValid { get; set; }

        public MessagePackSerializer<ResponseHeader> GetSerializer()
        {
            return ResponseHeader.Serializer;
        }
    }
    public class ArgumentHeader : IArgumentHeader, IMsgPackSerializable<ArgumentHeader>
    {
        public static readonly MessagePackSerializer<ArgumentHeader> Serializer = MessagePackSerializer.Get<ArgumentHeader>();

        public string Name { get; set; }

        public MessagePackSerializer<ArgumentHeader> GetSerializer()
        {
            return ArgumentHeader.Serializer;
        }
    }
    public class ModelHeader : IModelHeader, IMsgPackSerializable<ModelHeader>
    {
        public static readonly MessagePackSerializer<ModelHeader> Serializer = MessagePackSerializer.Get<ModelHeader>();

        public int PropertyCount { get; set; }

        public MessagePackSerializer<ModelHeader> GetSerializer()
        {
            return ModelHeader.Serializer;
        }
    }
    public class PropertyHeader : IPropertyHeader, IMsgPackSerializable<PropertyHeader>
    {
        public static readonly MessagePackSerializer<PropertyHeader> Serializer = MessagePackSerializer.Get<PropertyHeader>();

        public string Name { get; set; }
        public bool IsNull { get; set; }

        public MessagePackSerializer<PropertyHeader> GetSerializer()
        {
            return PropertyHeader.Serializer;
        }
    }
    public class ListHeader : IListHeader, IMsgPackSerializable<ListHeader>
    {
        public static readonly MessagePackSerializer<ListHeader> Serializer = MessagePackSerializer.Get<ListHeader>();

        public int ItemCount { get; set; }

        public MessagePackSerializer<ListHeader> GetSerializer()
        {
            return ListHeader.Serializer;
        }
    }
    public class Error : IError, IMsgPackSerializable<Error>
    {
        public static readonly MessagePackSerializer<Error> Serializer = MessagePackSerializer.Get<Error>();

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
