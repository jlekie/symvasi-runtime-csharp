using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Draft;

namespace Symvasi.Runtime.Acquisition.Etcd
{
    public class EtcdDiscoverer : ADiscoverer
    {
        protected Draft.IEtcdClient EtcdClient { get; private set; }

        public EtcdDiscoverer(IEndpointFactory endpointFactory, string serviceName, string etcdUrl)
            : base(endpointFactory, serviceName)
        {
            this.EtcdClient = Draft.Etcd.ClientFor(new Uri(etcdUrl));
        }

        public override async Task<IEnumerable<IEndpoint>> LoadEndpoints()
        {
            var result = (await this.EtcdClient
                .GetKey("/symvasi/endpoints/" + this.ServiceName)).Data;

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
