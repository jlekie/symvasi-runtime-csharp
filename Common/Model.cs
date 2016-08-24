using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

using Symvasi.Runtime.Protocol;

namespace Symvasi.Runtime
{
    public interface IModel
    {
        void Write(IProtocol protocol);
        void Read(IProtocol protocol, IModelHeader model);

        string GetModelName();

        string[] GetProperties();
        string[] GetProperties(string contractName);

        object GetPropertyValue(string propertyName);

        void SetPropertyValue(string propertyName, object value);
    }

    public abstract class AModel : IModel
    {
        public virtual void Write(IProtocol protocol)
        {
        }

        public void Read(IProtocol protocol, IModelHeader model)
        {
            for (var a = 0; a < model.PropertyCount; a++)
            {
                var prop = protocol.ReadModelPropertyStart();
                if (!this.ReadPropertyValue(protocol, prop))
                {
                    throw new Exception("Property '" + prop.Name + "' not recognized");
                }
                protocol.ReadModelPropertyEnd();
            }
        }

        protected virtual bool ReadPropertyValue(IProtocol protocol, IPropertyHeader prop)
        {
            return false;
        }

        protected void ValidateProp(IPropertyHeader prop, string type)
        {
        }

        public abstract string GetModelName();

        public abstract string[] GetProperties();
        public abstract string[] GetProperties(string contractName);

        public abstract object GetPropertyValue(string propertyName);
        public abstract void SetPropertyValue(string propertyName, object value);
    }
    [DataContract]
    public abstract class ADataContractModel : IModel
    {
        public virtual void Write(IProtocol protocol)
        {
        }

        public void Read(IProtocol protocol, IModelHeader model)
        {
            for (var a = 0; a < model.PropertyCount; a++)
            {
                var prop = protocol.ReadModelPropertyStart();
                if (!this.ReadPropertyValue(protocol, prop))
                {
                    throw new Exception("Property '" + prop.Name + "' not recognized");
                }
                protocol.ReadModelPropertyEnd();
            }
        }

        protected virtual bool ReadPropertyValue(IProtocol protocol, IPropertyHeader prop)
        {
            return false;
        }

        protected void ValidateProp(IPropertyHeader prop, string type)
        {
        }

        public abstract string GetModelName();

        public abstract string[] GetProperties();
        public abstract string[] GetProperties(string contractName);

        public abstract object GetPropertyValue(string propertyName);
        public abstract void SetPropertyValue(string propertyName, object value);
    }
}
