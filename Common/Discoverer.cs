using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Concurrent;

namespace Symvasi.Runtime.Acquisition
{
    public interface IDiscoverer<TEndpointFactory, TEndpoint>
        where TEndpointFactory : IEndpointFactory<TEndpoint>, new()
        where TEndpoint : IEndpoint
    {
        void Start();
        void Stop();

        void Refresh();

        TEndpoint GetEndpoint(TimeSpan? timeout = null, int retryInterval = 500);
        bool TryGetEndpoint(out TEndpoint endpoint, TimeSpan? timeout = null, int retryInterval = 500);
    }

    public abstract class ADiscoverer<TEndpointFactory, TEndpoint> : IDiscoverer<TEndpointFactory, TEndpoint>
        where TEndpointFactory : IEndpointFactory<TEndpoint>, new()
        where TEndpoint : IEndpoint
    {
        protected TEndpointFactory EndpointFactory { get; private set; }
        protected System.Collections.Concurrent.ConcurrentQueue<TEndpoint> Endpoints { get; private set; }

        private Task HandlerTask { get; set; }

        public ADiscoverer()
        {
            this.EndpointFactory = new TEndpointFactory();
            this.Endpoints = new System.Collections.Concurrent.ConcurrentQueue<TEndpoint>();
        }

        public void Start()
        {
            this.Refresh();

            this.HandlerTask = Task.Factory.StartNew(() => this.Handler(), TaskCreationOptions.LongRunning);
        }
        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            var endpointCount = this.Endpoints.Count;

            var endpoints = this.LoadEndpoints();
            foreach (var endpoint in endpoints)
            {
                if (!this.Endpoints.Contains(endpoint))
                {
                    this.Endpoints.Enqueue(endpoint);
                }
            }
        }

        private void Handler()
        {
            while (true)
            {
                try
                {
                    this.Refresh();

                    System.Threading.Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        public TEndpoint GetEndpoint(TimeSpan? timeout = null, int retryInterval = 500)
        {
            if (!timeout.HasValue)
                timeout = TimeSpan.FromSeconds(30);

            var startTime = DateTime.Now;

            TEndpoint endpoint;
            while (!this.Endpoints.TryDequeue(out endpoint))
            {
                if ((DateTime.Now - startTime) > timeout)
                    throw new Exception("Could not find backend endpoint.");

                System.Threading.Thread.Sleep(retryInterval);
            }
            this.Endpoints.Enqueue(endpoint);

            return endpoint;
        }
        public bool TryGetEndpoint(out TEndpoint endpoint, TimeSpan? timeout = null, int retryInterval = 500)
        {
            if (!timeout.HasValue)
                timeout = TimeSpan.FromSeconds(30);

            var startTime = DateTime.Now;

            while (!this.Endpoints.TryDequeue(out endpoint))
            {
                if ((DateTime.Now - startTime) > timeout)
                    return false;

                System.Threading.Thread.Sleep(retryInterval);
            }
            this.Endpoints.Enqueue(endpoint);

            return true;
        }

        protected abstract IEnumerable<TEndpoint> LoadEndpoints();
    }
}

namespace Symvasi.Runtime.Acquisition.Dictionary
{
    public class DictionaryDiscoverer<TEndpointFactory, TEndpoint> : ADiscoverer<TEndpointFactory, TEndpoint>
        where TEndpointFactory : IEndpointFactory<TEndpoint>, new()
        where TEndpoint : IEndpoint
    {
        public string ServiceName { get; private set; }
        protected ConcurrentDictionary<string, Dictionary<string, string>> Registry { get; private set; }

        public DictionaryDiscoverer(string serviceName, ConcurrentDictionary<string, Dictionary<string, string>> registry)
            : base()
        {
            this.ServiceName = serviceName;

            this.Registry = registry;
        }

        protected override IEnumerable<TEndpoint> LoadEndpoints()
        {
            Dictionary<string, string> endpoints;
            if (!this.Registry.TryGetValue(this.ServiceName, out endpoints))
                return new TEndpoint[] { };

            return endpoints.Values.Select(endpoint =>
            {
                var encodedData = System.Text.Encoding.UTF8.GetBytes(endpoint);

                return this.EndpointFactory.Load(encodedData);
            }).ToArray();
        }
    }
}