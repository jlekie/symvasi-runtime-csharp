using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Symvasi.Runtime.Transport;
using Symvasi.Runtime.Protocol;

namespace Symvasi.Runtime.Service
{
    public interface IServiceDirector<T> where T : IServiceServer
    {
        T CreateServer();
        T SpawnServer();
    }

    public abstract class AServiceDirector<T> : IServiceDirector<T> where T : IServiceServer
    {
        private Func<IServerTransport> TransportFactory { get; set; }
        private Func<IServerTransport, IServerProtocol> ProtocolFactory { get; set; }
        private Func<IServerTransport, IServerProtocol, T> ServerFactory { get; set; }

        public AServiceDirector(Func<IServerTransport> transportFactory, Func<IServerTransport, IServerProtocol> protocolFactory, Func<IServerTransport, IServerProtocol, T> serverFactory)
        {
            this.TransportFactory = transportFactory;
            this.ProtocolFactory = protocolFactory;
            this.ServerFactory = serverFactory;
        }

        public T CreateServer()
        {
            var transport = this.TransportFactory();
            var protocol = this.ProtocolFactory(transport);

            return this.ServerFactory(transport, protocol);
        }
        public T SpawnServer()
        {
            var server = this.CreateServer();
            server.Start();

            return server;
        }
    }
}
