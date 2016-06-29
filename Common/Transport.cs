using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symvasi.Runtime.Transport
{
    public interface ITransport
    {
        void Send(byte[] data);
        byte[] Receive();
    }
    public interface IServerTransport : ITransport
    {
        event EventHandler<ReceivedEventArgs> Received;

        IEndpoint Endpoint { get; }

        void Send(byte[] data, Guid schedulerId);
        byte[] Receive(Guid schedulerId);

        void Listen();
    }
    public interface IClientTransport : ITransport
    {
        IEndpoint Endpoint { get; }

        void Connect();
    }

    public abstract class AServerTransport : IServerTransport
    {
        public event EventHandler<ReceivedEventArgs> Received;

        public IEndpoint Endpoint { get; private set; }

        public AServerTransport(IEndpoint endpoint)
        {
            this.Endpoint = endpoint;
        }

        public abstract void Listen();

        public abstract void Send(byte[] data);
        public abstract void Send(byte[] data, Guid schedulerId);

        public abstract byte[] Receive();
        public abstract byte[] Receive(Guid schedulerId);

        protected virtual void OnReceived()
        {
            if (this.Received != null)
                this.Received(this, new ReceivedEventArgs());
        }
    }
    public abstract class AClientTransport : IClientTransport
    {
        public IEndpoint Endpoint { get; private set; }

        public AClientTransport(IEndpoint endpoint)
        {
            this.Endpoint = endpoint;
        }

        public abstract void Connect();

        public abstract void Send(byte[] data);
        public abstract byte[] Receive();
    }

    public class ReceivedEventArgs : EventArgs
    {
        public ReceivedEventArgs()
            : base()
        {
        }
    }
}
