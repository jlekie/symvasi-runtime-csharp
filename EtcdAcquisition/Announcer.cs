using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Draft;

namespace Symvasi.Runtime.Acquisition.Etcd
{
    public class EtcdAnnouncer : AAnnouncer
    {
        protected Draft.IEtcdClient EtcdClient { get; private set; }

        public EtcdAnnouncer(string serviceName, string etcdUrl)
            : base(serviceName)
        {
            this.EtcdClient = Draft.Etcd.ClientFor(new Uri(etcdUrl));
        }

        public override async Task Register(IEndpoint endpoint)
        {
            var savedEndpoint = endpoint.Save();
            var decodedData = System.Text.Encoding.UTF8.GetString(savedEndpoint.Data);

            await this.EtcdClient.UpsertKey(string.Format("/symvasi/endpoints/{0}/{1}", this.ServiceName, savedEndpoint.Id))
                .WithValue(decodedData).WithTimeToLive(20);
        }
    }
}
