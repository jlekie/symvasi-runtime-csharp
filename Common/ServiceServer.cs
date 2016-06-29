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
        void HandleRequest(IRequestHeader request, IServerProtocol protocol);

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

        public abstract void HandleRequest(IRequestHeader request, IServerProtocol protocol);

        public void Start()
        {
            this.Transport.Listen();

            this.ServerTask = Task.Factory.StartNew(() => this.ServerHandler(), TaskCreationOptions.LongRunning);
        }
        public void Stop()
        {
            throw new NotImplementedException();
        }

        private void ServerHandler()
        {
            while (true)
            {
                try
                {
                    var request = this.Protocol.ReadRequestStart();
                    this.OnReceivedRequest(request, this.Protocol);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        protected virtual void OnReceivedRequest(IRequestHeader request, IServerProtocol protocol)
        {
            this.HandleRequest(request, protocol);
        }
    }
    public abstract class AAsyncServiceServer : IServiceServer
    {
        public IServerTransport Transport { get; private set; }
        protected Func<IServerTransport, Guid, IServerProtocol> ProtocolFactory { get; private set; }

        public AAsyncServiceServer(IServerTransport transport, Func<IServerTransport, Guid, IServerProtocol> protocolFactory)
        {
            this.Transport = transport;
            this.ProtocolFactory = protocolFactory;

            this.Transport.Received += Transport_Received;
        }

        public abstract void HandleRequest(IRequestHeader request, IServerProtocol protocol);

        public void Start()
        {
            this.Transport.Listen();
        }
        public void Stop()
        {
            throw new NotImplementedException();
        }

        private void Transport_Received(object sender, ReceivedEventArgs e)
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    var schedulerId = Guid.NewGuid();
                    var protocol = this.ProtocolFactory(this.Transport, schedulerId);

                    var request = protocol.ReadRequestStart();
                    this.OnReceivedRequest(request, protocol);
                }, TaskCreationOptions.LongRunning).ContinueWith((task) =>
                {
                    if (task.IsFaulted)
                    {
                        throw new Exception("Async server failure", task.Exception);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        protected virtual void OnReceivedRequest(IRequestHeader request, IServerProtocol protocol)
        {
            // Testabc123abcabc123abc
            this.HandleRequest(request, protocol);
        }
    }
}
