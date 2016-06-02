using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Symvasi.Runtime.Transport;
using Symvasi.Runtime.Protocol;

namespace Symvasi.Runtime.Service
{
    public interface IServiceConsumer<T> where T : IServiceClient
    {
        T CreateClient();
        T SpawnClient();
    }

    public abstract class AServiceConsumer<T> : IServiceConsumer<T> where T : IServiceClient
    {
        private Func<IClientTransport> TransportFactory { get; set; }
        private Func<IClientTransport, IClientProtocol> ProtocolFactory { get; set; }
        private Func<IClientTransport, IClientProtocol, T> ClientFactory { get; set; }

        public AServiceConsumer(Func<IClientTransport> transportFactory, Func<IClientTransport, IClientProtocol> protocolFactory, Func<IClientTransport, IClientProtocol, T> clientFactory)
        {
            this.TransportFactory = transportFactory;
            this.ProtocolFactory = protocolFactory;
            this.ClientFactory = clientFactory;
        }

        public T CreateClient()
        {
            var transport = this.TransportFactory();
            var protocol = this.ProtocolFactory(transport);

            return this.ClientFactory(transport, protocol);
        }
        public T SpawnClient()
        {
            var client = this.CreateClient();
            client.Connect();

            return client;
        }
    }
}
