using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Concurrent;

namespace Symvasi.Runtime.Acquisition
{
    public interface IAnnouncer<TEndpoint> where TEndpoint : IEndpoint
    {
        void Start();
        void Stop();

        void Refresh();
    }

    public abstract class AAnnouncer<TEndpoint> : IAnnouncer<TEndpoint> where TEndpoint : IEndpoint
    {
        public TEndpoint Endpoint { get; private set; }
        public string ServiceName { get; private set; }

        private Task HandlerTask { get; set; }

        public AAnnouncer(TEndpoint endpoint, string serviceName)
        {
            this.Endpoint = endpoint;
            this.ServiceName = serviceName;
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
            this.Register();
        }

        private void Handler()
        {
            while (true)
            {
                try
                {
                    this.Refresh();

                    System.Threading.Thread.Sleep(10000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        protected abstract void Register();
    }
}

namespace Symvasi.Runtime.Acquisition.Dictionary
{
    public class DictionaryAnnouncer<TEndpoint> : AAnnouncer<TEndpoint> where TEndpoint : IEndpoint
    {
        protected ConcurrentDictionary<string, Dictionary<string, string>> Registry { get; private set; }

        public DictionaryAnnouncer(TEndpoint endpoint, string serviceName, ConcurrentDictionary<string, Dictionary<string, string>> registry)
            : base(endpoint, serviceName)
        {
            this.Registry = registry;
        }

        protected override void Register()
        {
            var savedEndpoint = this.Endpoint.Save();
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
        }
    }
}
