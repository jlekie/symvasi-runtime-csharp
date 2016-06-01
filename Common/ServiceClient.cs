using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Symvasi.Runtime.Transport;
using Symvasi.Runtime.Protocol;

namespace Symvasi.Runtime.Service
{
    public interface IServiceClient
    {
        void Connect();
    }

    public abstract class AServiceClient : IServiceClient
    {
        public IClientTransport Transport { get; private set; }
        public IClientProtocol Protocol { get; private set; }

        public AServiceClient(IClientTransport transport, IClientProtocol protocol)
        {
            this.Transport = transport;
            this.Protocol = protocol;
        }

        public virtual void Connect()
        {
            this.Transport.Connect();
        }
    }
}
