using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Concurrent;

namespace Symvasi.Runtime.Acquisition
{
    public interface IDiscoverer
    {
        Task<IEnumerable<IEndpoint>> LoadEndpoints();

        Task<IEndpoint> GetEndpoint(TimeSpan? timeout = null, int retryInterval = 500);
    }

    public abstract class ADiscoverer : IDiscoverer
    {
        public string ServiceName { get; private set; }
        protected IEndpointFactory EndpointFactory { get; private set; }

        public ADiscoverer(IEndpointFactory endpointFactory, string serviceName)
        {
            this.EndpointFactory = endpointFactory;
        }

        public abstract Task<IEnumerable<IEndpoint>> LoadEndpoints();
        public async Task<IEndpoint> GetEndpoint(TimeSpan? timeout = null, int retryInterval = 500)
        {
            if (!timeout.HasValue)
                timeout = TimeSpan.FromSeconds(30);

            var startTime = DateTime.Now;

            var endpoints = await this.LoadEndpoints();
            while (!endpoints.Any())
            {
                if ((DateTime.Now - startTime) > timeout)
                    throw new Exception("Could not find backend endpoint.");

                System.Threading.Thread.Sleep(retryInterval);

                endpoints = await this.LoadEndpoints();
            }

            return endpoints.FirstOrDefault();
        }
    }
}

namespace Symvasi.Runtime.Acquisition.Dictionary
{
    public class DictionaryDiscoverer : ADiscoverer
    {
        protected ConcurrentDictionary<string, Dictionary<string, string>> Registry { get; private set; }

        public DictionaryDiscoverer(IEndpointFactory endpointFactory, string serviceName, ConcurrentDictionary<string, Dictionary<string, string>> registry)
            : base(endpointFactory, serviceName)
        {
            this.Registry = registry;
        }

        public override Task<IEnumerable<IEndpoint>> LoadEndpoints()
        {
            IEnumerable<IEndpoint> result;

            Dictionary<string, string> endpoints;
            if (!this.Registry.TryGetValue(this.ServiceName, out endpoints))
                result = new IEndpoint[] { };

            result = endpoints.Values.Select(endpoint =>
            {
                var encodedData = System.Text.Encoding.UTF8.GetBytes(endpoint);

                return this.EndpointFactory.Load(encodedData);
            }).ToArray();

            return Task.FromResult(result);
        }
    }
}