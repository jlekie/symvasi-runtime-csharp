using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symvasi.Runtime.Transport.ZeroMQ
{
    public class ZeroMQEndpointFactory : IEndpointFactory<IZeroMQEndpoint>
    {
        public IZeroMQEndpoint Load(byte[] data)
        {
            var decodedData = System.Text.Encoding.UTF8.GetString(data);

            var facets = decodedData.Split(new string[] { "|" }, StringSplitOptions.None);

            switch (facets[0])
            {
                case "tcp":
                    return TcpEndpoint.Load(facets);
                default:
                    throw new Exception(string.Format("Unknown endpoint type '{0}'", facets[0]));
            }
        }
    }
}
