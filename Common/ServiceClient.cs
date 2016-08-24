using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Symvasi.Runtime.Transport;
using Symvasi.Runtime.Protocol;
using Symvasi.Runtime.Acquisition;

namespace Symvasi.Runtime.Service
{
    public interface IServiceClient
    {
        ITransport Transport { get; }
        IProtocol Protocol { get; }

        Task Connect(IDiscoverer discoverer);
        Task Connect(IEndpoint endpoint);
    }

    public abstract class AServiceClient : IServiceClient
    {
        public ITransport Transport { get; private set; }
        public IProtocol Protocol { get; private set; }

        public AServiceClient(ITransport transport, IProtocol protocol)
        {
            this.Transport = transport;
            this.Protocol = protocol;
        }

        public async Task Connect(IDiscoverer discoverer)
        {
            var endpoint = await discoverer.GetEndpoint();

            await this.Transport.Connect(endpoint);
        }
        public async Task Connect(IEndpoint endpoint)
        {
            await this.Transport.Connect(endpoint);
        }
    }
}
