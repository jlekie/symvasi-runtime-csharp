using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Symvasi.Runtime.Transport;
using Symvasi.Runtime.Protocol;
using Symvasi.Runtime.Acquisition;

namespace Symvasi.Runtime.Service
{
    public interface IServiceServer
    {
        ITransport Transport { get; }
        IProtocol Protocol { get; }

        Task Start(IAnnouncer announcer = null);
        void Stop();

        Task HandleRequest();
    }

    public abstract class AServiceServer : IServiceServer
    {
        public ITransport Transport { get; private set; }
        public IProtocol Protocol { get; private set; }

        public bool Running { get; private set; }

        private Task ServerTask { get; set; }
        private Task AnnouncerTask { get; set; }

        public AServiceServer(ITransport transport, IProtocol protocol)
        {
            this.Transport = transport;
            this.Protocol = protocol;
        }

        public async Task Start(IAnnouncer announcer = null)
        {
            this.Running = true;

            var endpoint = await this.Transport.Listen();
            if (announcer != null)
            {
                await announcer.Register(endpoint);
                this.AnnouncerTask = Task.Factory.StartNew(() => this.AnnouncerHandler(announcer, endpoint), TaskCreationOptions.LongRunning);
            }

            this.ServerTask = Task.Factory.StartNew(() => this.ServerHandler(), TaskCreationOptions.LongRunning);
        }
        public void Stop()
        {
            this.Running = false;
        }

        private void ServerHandler()
        {
            while (this.Running)
            {
                try
                {
                    this.HandleRequest().Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }
        private void AnnouncerHandler(IAnnouncer announcer, IEndpoint endpoint)
        {
            while (this.Running)
            {
                System.Threading.Thread.Sleep(5000);

                try
                {
                    announcer.Register(endpoint).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Announcer Error: " + ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        public abstract Task HandleRequest();
    }
}
