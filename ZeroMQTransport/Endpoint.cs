using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Symvasi.Runtime.Acquisition;

namespace Symvasi.Runtime.Transport.ZeroMQ
{
    public interface IZeroMQEndpoint : IEndpoint
    {
        string ToServerConnectionString();
        string ToClientConnectionString();
    }
    public interface ITcpEndpoint : IZeroMQEndpoint
    {
        string Address { get; }
        int Port { get; }
    }
    public interface IInprocEndpoint : IZeroMQEndpoint
    {
        string Address { get; }
    }

    public abstract class AZeroMQEndpoint : AEndpoint, IZeroMQEndpoint
    {
        public abstract string ToServerConnectionString();
        public abstract string ToClientConnectionString();
    }

    public class TcpEndpoint : AZeroMQEndpoint, ITcpEndpoint
    {
        internal static TcpEndpoint Load(string[] facets)
        {
            var address = facets[1];
            var port = int.Parse(facets[2]);

            return new TcpEndpoint(address, port);
        }

        public static TcpEndpoint Allocate(string ifaceName, int port = 0)
        {
            string address;

            var iface = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .SingleOrDefault(i => i.Name == ifaceName);
            if (iface == null)
                throw new Exception(string.Format("Interface '{0}' does not exist", ifaceName));
            if (iface.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                throw new Exception(string.Format("Interface '{0}' is not connected", ifaceName));

            var ipAddresses = iface.GetIPProperties().UnicastAddresses;
            var ipv4Address = ipAddresses.FirstOrDefault(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            if (ipv4Address == null)
                throw new Exception(string.Format("Interface '{0}' does not have a valid IPv4 address", ifaceName));

            address = ipv4Address.Address.ToString();

            if (port == 0)
            {
                var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
                listener.Start();
                port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
                listener.Stop();
            }

            return new TcpEndpoint(address, port);
        }

        public string Address { get; private set; }
        public int Port { get; private set; }

        public TcpEndpoint(string address, int port)
            : base()
        {
            this.Address = address;
            this.Port = port;
        }

        public override string ToServerConnectionString()
        {
            return string.Format("tcp://*:{0}", this.Port);
        }
        public override string ToClientConnectionString()
        {
            return string.Format("tcp://{0}:{1}", this.Address, this.Port);
        }

        public override SavedEndpoint Save()
        {
            var decodedData = string.Format("{0}|{1}|{2}", "tcp", this.Address, this.Port);

            return new SavedEndpoint(string.Format("{0}:{1}:{2}", "tcp", this.Address, this.Port), System.Text.Encoding.UTF8.GetBytes(decodedData));
        }

        public override bool Equals(object obj)
        {
            var compObj = obj as TcpEndpoint;
            if (compObj != null)
            {
                return this.Address == compObj.Address && this.Port == compObj.Port;
            }

            return false;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                const int HashingBase = (int)77924713333;
                const int HashingMultiplier = (int)138283;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ (this.Address != null ? this.Address.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (this.Port.GetHashCode());

                return hash;
            }
        }
    }
    public class InprocEndpoint : AZeroMQEndpoint, IInprocEndpoint
    {
        private static Dictionary<string, int> AddressRegistry = new Dictionary<string, int>();

        internal static InprocEndpoint Load(string[] facets)
        {
            var address = facets[1];

            return new InprocEndpoint(address);
        }

        public static InprocEndpoint Allocate(string address)
        {
            if (!AddressRegistry.ContainsKey(address))
            {
                AddressRegistry.Add(address, 1);
            }

            var index = AddressRegistry[address]++;

            return new InprocEndpoint(address + "-" + index);
        }

        public string Address { get; private set; }
        
        public InprocEndpoint(string address)
            : base()
        {
            this.Address = address;
        }

        public override string ToServerConnectionString()
        {
            return string.Format("inproc://{0}", this.Address);
        }
        public override string ToClientConnectionString()
        {
            return string.Format("inproc://{0}", this.Address);
        }

        public override SavedEndpoint Save()
        {
            var decodedData = string.Format("{0}|{1}", "inproc", this.Address);

            return new SavedEndpoint(string.Format("{0}:{1}", "tcp", this.Address), System.Text.Encoding.UTF8.GetBytes(decodedData));
        }

        public override bool Equals(object obj)
        {
            var compObj = obj as InprocEndpoint;
            if (compObj != null)
            {
                return this.Address == compObj.Address;
            }

            return false;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                const int HashingBase = (int)437272603;
                const int HashingMultiplier = (int)7579769;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ (this.Address != null ? this.Address.GetHashCode() : 0);

                return hash;
            }
        }
    }
}
