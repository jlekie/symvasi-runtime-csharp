using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Symvasi.Runtime.Acquisition;

namespace Symvasi.Runtime.Transport.ZeroMQ
{
    public interface IZeroMQServerEndpoint : IServerEndpoint
    {
        string ToServerConnectionString();
        string ToClientConnectionString();
    }
    public interface IZeroMQClientEndpoint : IClientEndpoint
    {
        string ToConnectionString();
    }

    public class TcpServerEndpoint : IZeroMQServerEndpoint
    {
        internal static TcpServerEndpoint Load(string[] facets)
        {
            var address = facets[1];
            var port = int.Parse(facets[2]);

            return new TcpServerEndpoint(address, port);
        }

        public string Address { get; private set; }
        public int Port { get; private set; }

        public TcpServerEndpoint(string address, int port)
            : base()
        {
            this.Address = address;
            this.Port = port;
        }

        public string ToServerConnectionString()
        {
            return string.Format("tcp://*:{0}", this.Port);
        }
        public string ToClientConnectionString()
        {
            return string.Format("tcp://{0}:{1}", this.Address, this.Port);
        }

        public byte[] Save()
        {
            var decodedData = string.Format("{0}|{1}|{2}", "tcp", this.Address, this.Port);

            return System.Text.Encoding.UTF8.GetBytes(decodedData);
        }
    }
    public class DiscoveryServerEndpoint : IZeroMQServerEndpoint
    {
        public IDiscoverer<ZeroMQServerEndpointFactory, IZeroMQServerEndpoint> Discoverer { get; private set; }

        public DiscoveryServerEndpoint(IDiscoverer<ZeroMQServerEndpointFactory, IZeroMQServerEndpoint> discoverer)
        {
            this.Discoverer = discoverer;
        }

        public string ToClientConnectionString()
        {
            var endpoint = this.Discoverer.GetEndpoint();

            return endpoint.ToClientConnectionString();
        }
        public string ToServerConnectionString()
        {
            var endpoint = this.Discoverer.GetEndpoint();

            return endpoint.ToServerConnectionString();
        }

        public byte[] Save()
        {
            throw new NotImplementedException();
        }
    }

    public class TcpClientEndpoint : IZeroMQClientEndpoint
    {
        internal static TcpClientEndpoint Load(string[] facets)
        {
            var address = facets[1];
            var port = int.Parse(facets[2]);

            return new TcpClientEndpoint(address, port);
        }

        public string Address { get; private set; }
        public int Port { get; private set; }

        public TcpClientEndpoint(string address, int port)
            : base()
        {
            this.Address = address;
            this.Port = port;
        }

        public string ToConnectionString()
        {
            return string.Format("tcp://{0}:{1}", this.Address, this.Port);
        }
    }
    public class DiscoveryClientEndpoint : IZeroMQClientEndpoint
    {
        public IDiscoverer<ZeroMQClientEndpointFactory, IZeroMQClientEndpoint> Discoverer { get; private set; }

        public DiscoveryClientEndpoint(IDiscoverer<ZeroMQClientEndpointFactory, IZeroMQClientEndpoint> discoverer)
        {
            this.Discoverer = discoverer;
        }

        public string ToConnectionString()
        {
            var endpoint = this.Discoverer.GetEndpoint();

            return endpoint.ToConnectionString();
        }
    }
}
