using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Symvasi.Runtime.Transport;
using Symvasi.Runtime.Protocol;

namespace Symvasi.Runtime.Service
{
    public interface IServiceServer
    {
        void HandleRequest(IRequestHeader request);

        void Start();
        void Stop();
    }

    public abstract class AServiceServer : IServiceServer
    {
        public IServerTransport Transport { get; private set; }
        public IServerProtocol Protocol { get; private set; }

        private Task ServerTask { get; set; }

        public AServiceServer(IServerTransport transport, IServerProtocol protocol)
        {
            this.Transport = transport;
            this.Protocol = protocol;
        }

        public abstract void HandleRequest(IRequestHeader request);

        public void Start()
        {
            this.Transport.Listen();

            this.ServerTask = Task.Factory.StartNew(() => this.ServerHandler(), TaskCreationOptions.LongRunning);
        }
        public void Stop()
        {
        }

        private void ServerHandler()
        {
            while (true)
            {
                try
                {
                    var request = this.Protocol.ReadRequestStart();
                    this.HandleRequest(request);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
    }
}
