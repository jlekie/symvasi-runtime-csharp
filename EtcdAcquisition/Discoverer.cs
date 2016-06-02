using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Draft;

namespace Symvasi.Runtime.Acquisition.Etcd
{
    public class EtcdDiscoverer<TEndpointFactory, TEndpoint> : ADiscoverer<TEndpointFactory, TEndpoint>
        where TEndpointFactory : IEndpointFactory<TEndpoint>, new()
        where TEndpoint : IEndpoint
    {
        public string ServiceName { get; private set; }
        protected Draft.IEtcdClient EtcdClient { get; private set; }

        public EtcdDiscoverer(string serviceName, string etcdUrl)
            : base()
        {
            this.ServiceName = serviceName;

            this.EtcdClient = Draft.Etcd.ClientFor(new Uri(etcdUrl));
        }

        protected override IEnumerable<TEndpoint> LoadEndpoints()
        {
            var data = this.EtcdClient
                .GetKey("/symvasi/endpoints/" + this.ServiceName)
                .GetAwaiter().GetResult().Data.RawValue;
            var encodedData = System.Text.Encoding.UTF8.GetBytes(data);
            
            return new TEndpoint[] { this.EndpointFactory.Load(encodedData) };
        }
    }
}
