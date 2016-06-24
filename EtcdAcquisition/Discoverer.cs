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
            var result = this.EtcdClient
                .GetKey("/symvasi/endpoints/" + this.ServiceName)
                .GetAwaiter().GetResult().Data;

            if (!result.IsDir)
                throw new Exception(string.Format("Invalid key (not directory) '{0}'", "/symvasi/endpoints/" + this.ServiceName));

            return result.Children.Select(child =>
            {
                var encodedData = System.Text.Encoding.UTF8.GetBytes(child.RawValue);

                return this.EndpointFactory.Load(encodedData);
            }).ToArray();
        }
    }
}
