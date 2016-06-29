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

        string[] GetProperties();
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

        public abstract string[] GetProperties();
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

        public abstract string[] GetProperties();
    }
}
