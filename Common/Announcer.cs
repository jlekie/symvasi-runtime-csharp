using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Concurrent;

namespace Symvasi.Runtime.Acquisition
{
    public interface IAnnouncer
    {
        Task Register(IEndpoint endpoint);
    }

    public abstract class AAnnouncer : IAnnouncer
    {
        public string ServiceName { get; private set; }

        public AAnnouncer(string serviceName)
        {
            this.ServiceName = serviceName;
        }

        public abstract Task Register(IEndpoint endpoint);
    }
}

namespace Symvasi.Runtime.Acquisition.Dictionary
{
    public class DictionaryAnnouncer : AAnnouncer
    {
        protected ConcurrentDictionary<string, Dictionary<string, string>> Registry { get; private set; }

        public DictionaryAnnouncer(string serviceName, ConcurrentDictionary<string, Dictionary<string, string>> registry)
            : base(serviceName)
        {
            this.Registry = registry;
        }

        public override Task Register(IEndpoint endpoint)
        {
            var savedEndpoint = endpoint.Save();
            var decodedData = System.Text.Encoding.UTF8.GetString(savedEndpoint.Data);

            this.Registry.AddOrUpdate(this.ServiceName, (serviceName) =>
            {
                var endpoints = new Dictionary<string, string>();
                endpoints.Add(savedEndpoint.Id, decodedData);

                return endpoints;
            }, (serviceName, endpoints) =>
            {
                if (endpoints.ContainsKey(savedEndpoint.Id))
                {
                    endpoints[savedEndpoint.Id] = decodedData;
                }
                else
                {
                    endpoints.Add(savedEndpoint.Id, decodedData);
                }

                return endpoints;
            });

            return Task.FromResult(0);
        }
    }
}
