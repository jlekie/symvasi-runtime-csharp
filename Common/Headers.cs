using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symvasi.Runtime.Protocol
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
        IndefinateEnd = 13,

        MapStart = 14,
        MapEnd = 15
    }

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

    public interface IHeader
    {
        void Read(IProtocol protocol);
        void Write(IProtocol protocol);
    }
    public interface IArgumentHeader : IHeader
    {
        string Name { get; }
    }
    public interface IListHeader : IHeader
    {
        int ItemCount { get; }
    }
    public interface IMapHeader : IHeader
    {
        int ItemCount { get; }
    }
    public interface IModelHeader : IHeader
    {
        int PropertyCount { get; }
    }
    public interface IAbstractModelHeader : IModelHeader
    {
        string DerivedModelName { get; }
    }
    public interface IIndefinateHeader : IHeader
    {
        IndefinateTypes Type { get; }
        string DeclaredType { get; }
    }
    public interface IPropertyHeader : IHeader
    {
        string Name { get; }
        bool IsNull { get; }
    }
    public interface IRequestHeader : IHeader
    {
        string MethodName { get; }
        int ArgumentCount { get; }

        Dictionary<string, string> Tags { get; }
    }
    public interface IResponseHeader : IHeader
    {
        bool IsValid { get; }
    }
    public interface IErrorHeader : IHeader
    {
        string Message { get; }
        string StackTrace { get; }

        Exception CreateException();
    }

    public abstract class AHeader : IHeader
    {
        public abstract void Read(IProtocol protocol);
        public abstract void Write(IProtocol protocol);
    }
    public class RequestHeader : AHeader, IRequestHeader
    {
        public string MethodName { get; set; }
        public int ArgumentCount { get; set; }
        public Dictionary<string, string> Tags { get; set; }

        public RequestHeader()
            : base()
        {
            this.Tags = new Dictionary<string, string>();
        }
        public RequestHeader(string methodName, int argumentCount, IDictionary<string, string> tags = null)
            : this()
        {
            this.MethodName = methodName;
            this.ArgumentCount = argumentCount;

            if (tags != null)
            {
                foreach (var tag in tags)
                    this.Tags.Add(tag.Key, tag.Value);
            }
        }

        public override void Read(IProtocol protocol)
        {
            this.MethodName = protocol.ReadStringValue();
            this.ArgumentCount = protocol.ReadIntegerValue();

            var tagsHeader = protocol.ReadMapStart();
            for (var a = 0; a < tagsHeader.ItemCount; a++)
            {
                var key = protocol.ReadStringValue();
                var value = protocol.ReadStringValue();

                this.Tags.Add(key, value);
            }
            protocol.ReadMapEnd();
        }
        public override void Write(IProtocol protocol)
        {
            protocol.WriteStringValue(this.MethodName);
            protocol.WriteIntegerValue(this.ArgumentCount);

            protocol.WriteMapStart(this.Tags.Count);
            foreach (var tag in this.Tags)
            {
                protocol.WriteStringValue(tag.Key);
                protocol.WriteStringValue(tag.Value);
            }
            protocol.WriteMapEnd();
        }
    }
    public class ResponseHeader : AHeader, IResponseHeader
    {
        public bool IsValid { get; set; }

        public ResponseHeader()
            : base()
        {
        }
        public ResponseHeader(bool isValid)
            : this()
        {
            this.IsValid = isValid;
        }

        public override void Read(IProtocol protocol)
        {
            this.IsValid = protocol.ReadBoolValue();
        }
        public override void Write(IProtocol protocol)
        {
            protocol.WriteBoolValue(this.IsValid);
        }
    }
    public class ArgumentHeader : AHeader, IArgumentHeader
    {
        public string Name { get; set; }

        public ArgumentHeader()
            : base()
        {
        }
        public ArgumentHeader(string name)
            : this()
        {
            this.Name = name;
        }

        public override void Read(IProtocol protocol)
        {
            this.Name = protocol.ReadStringValue();
        }
        public override void Write(IProtocol protocol)
        {
            protocol.WriteStringValue(this.Name);
        }
    }
    public class ModelHeader : AHeader, IModelHeader
    {
        public int PropertyCount { get; set; }

        public ModelHeader()
            : base()
        {
        }
        public ModelHeader(int propertyCount)
            : this()
        {
            this.PropertyCount = propertyCount;
        }

        public override void Read(IProtocol protocol)
        {
            this.PropertyCount = protocol.ReadIntegerValue();
        }
        public override void Write(IProtocol protocol)
        {
            protocol.WriteIntegerValue(this.PropertyCount);
        }
    }
    public class AbstractModelHeader : ModelHeader, IAbstractModelHeader
    {
        public string DerivedModelName { get; set; }

        public AbstractModelHeader()
            : base()
        {
        }
        public AbstractModelHeader(int propertyCount)
            : base(propertyCount)
        {
        }
        public AbstractModelHeader(int propertyCount, string derivedModelName)
            : this(propertyCount)
        {
            this.DerivedModelName = derivedModelName;
        }

        public override void Read(IProtocol protocol)
        {
            base.Read(protocol);

            this.DerivedModelName = protocol.ReadStringValue();
        }
        public override void Write(IProtocol protocol)
        {
            base.Write(protocol);

            protocol.WriteStringValue(this.DerivedModelName);
        }
    }
    public class PropertyHeader : AHeader, IPropertyHeader
    {
        public string Name { get; set; }
        public bool IsNull { get; set; }

        public PropertyHeader()
            : base()
        {
        }
        public PropertyHeader(string name, bool isNull)
            : this()
        {
            this.Name = name;
            this.IsNull = isNull;
        }

        public override void Read(IProtocol protocol)
        {
            this.Name = protocol.ReadStringValue();
            this.IsNull = protocol.ReadBoolValue();
        }
        public override void Write(IProtocol protocol)
        {
            protocol.WriteStringValue(this.Name);
            protocol.WriteBoolValue(this.IsNull);
        }
    }
    public class ListHeader : AHeader, IListHeader
    {
        public int ItemCount { get; set; }

        public ListHeader()
            : base()
        {
        }
        public ListHeader(int itemCount)
            : this()
        {
            this.ItemCount = itemCount;
        }

        public override void Read(IProtocol protocol)
        {
            this.ItemCount = protocol.ReadIntegerValue();
        }
        public override void Write(IProtocol protocol)
        {
            protocol.WriteIntegerValue(this.ItemCount);
        }
    }
    public class MapHeader : AHeader, IMapHeader
    {
        public int ItemCount { get; set; }

        public MapHeader()
            : base()
        {
        }
        public MapHeader(int itemCount)
            : this()
        {
            this.ItemCount = itemCount;
        }

        public override void Read(IProtocol protocol)
        {
            this.ItemCount = protocol.ReadIntegerValue();
        }
        public override void Write(IProtocol protocol)
        {
            protocol.WriteIntegerValue(this.ItemCount);
        }
    }
    public class IndefinateHeader : AHeader, IIndefinateHeader
    {
        public IndefinateTypes Type { get; set; }
        public string DeclaredType { get; set; }

        public IndefinateHeader()
            : base()
        {
        }
        public IndefinateHeader(IndefinateTypes type, string declaredType)
            : this()
        {
            this.Type = type;
            this.DeclaredType = declaredType;
        }

        public override void Read(IProtocol protocol)
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
        public override void Write(IProtocol protocol)
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
    public class ErrorHeader : AHeader, IErrorHeader
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }

        public ErrorHeader()
            : base()
        {
        }
        public ErrorHeader(string message)
            : this()
        {
            this.Message = message;
        }
        public ErrorHeader(Exception ex)
            : this()
        {
            this.Message = ex.Message;
            this.StackTrace = ex.StackTrace;
        }

        public Exception CreateException()
        {
            return new Exception(this.Message);
        }

        public override void Read(IProtocol protocol)
        {
            this.Message = protocol.ReadStringValue();
            this.StackTrace = protocol.ReadStringValue();
        }
        public override void Write(IProtocol protocol)
        {
            protocol.WriteStringValue(this.Message);
            protocol.WriteStringValue(this.StackTrace);
        }
    }
}
