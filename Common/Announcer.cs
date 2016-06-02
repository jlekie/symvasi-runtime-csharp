using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symvasi.Runtime.Acquisition
{
    public interface IAnnouncer<TEndpoint> where TEndpoint : IServerEndpoint
    {
        void Start();
        void Stop();

        void Refresh();
    }

    public abstract class AAnnouncer<TEndpoint> : IAnnouncer<TEndpoint> where TEndpoint : IServerEndpoint
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

                    System.Threading.Thread.Sleep(30000);
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
