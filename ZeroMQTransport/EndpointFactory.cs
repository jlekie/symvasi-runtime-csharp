using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symvasi.Runtime.Transport.ZeroMQ
{
    public class ZeroMQServerEndpointFactory : IEndpointFactory<IZeroMQServerEndpoint>
    {
        public IZeroMQServerEndpoint Load(byte[] data)
        {
            var decodedData = System.Text.Encoding.UTF8.GetString(data);

            var facets = decodedData.Split(new string[] { "|" }, StringSplitOptions.None);

            switch (facets[0])
            {
                case "tcp":
                    return TcpServerEndpoint.Load(facets);
                default:
                    throw new Exception(string.Format("Unknown endpoint type '{0}'", facets[0]));
            }
        }
    }
    public class ZeroMQClientEndpointFactory : IEndpointFactory<IZeroMQClientEndpoint>
    {
        public IZeroMQClientEndpoint Load(byte[] data)
        {
            var decodedData = System.Text.Encoding.UTF8.GetString(data);

            var facets = decodedData.Split(new string[] { "|" }, StringSplitOptions.None);

            switch (facets[0])
            {
                case "tcp":
                    return TcpClientEndpoint.Load(facets);
                default:
                    throw new Exception(string.Format("Unknown endpoint type '{0}'", facets[0]));
            }
        }
    }
}
