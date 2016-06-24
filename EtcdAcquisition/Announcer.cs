using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Draft;

namespace Symvasi.Runtime.Acquisition.Etcd
{
    public class EtcdAnnouncer<TEndpoint> : AAnnouncer<TEndpoint> where TEndpoint : IEndpoint
    {
        protected Draft.IEtcdClient EtcdClient { get; private set; }

        public EtcdAnnouncer(TEndpoint endpoint, string serviceName, string etcdUrl)
            : base(endpoint, serviceName)
        {
            this.EtcdClient = Draft.Etcd.ClientFor(new Uri(etcdUrl));
        }

        protected override void Register()
        {
            var savedEndpoint = this.Endpoint.Save();
            var decodedData = System.Text.Encoding.UTF8.GetString(savedEndpoint.Data);

            this.EtcdClient.UpsertKey(string.Format("/symvasi/endpoints/{0}/{1}", this.ServiceName, savedEndpoint.Id))
                .WithValue(decodedData).WithTimeToLive(20).GetAwaiter().GetResult();
        }
    }
}
