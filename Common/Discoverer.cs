using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symvasi.Runtime.Acquisition
{
    public interface IDiscoverer<TEndpointFactory, TEndpoint>
        where TEndpointFactory : IEndpointFactory<TEndpoint>, new()
        where TEndpoint : IEndpoint
    {
        void Start();
        void Stop();

        void Refresh();

        TEndpoint GetEndpoint();
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
            var endpoints = this.LoadEndpoints();
            foreach (var endpoint in endpoints)
            {
                this.Endpoints.Enqueue(endpoint);
            }
        }

        private void Handler()
        {
            while (true)
            {
                try
                {
                    this.Refresh();

                    System.Threading.Thread.Sleep(30000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        public TEndpoint GetEndpoint()
        {
            TEndpoint endpoint;
            this.Endpoints.TryDequeue(out endpoint);
            this.Endpoints.Enqueue(endpoint);

            return endpoint;
        }

        protected abstract IEnumerable<TEndpoint> LoadEndpoints();
    }
}
