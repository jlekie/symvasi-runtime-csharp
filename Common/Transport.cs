using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symvasi.Runtime.Transport
{
    public interface ITransport
    {
        event EventHandler<ReceivedEventArgs> Received;

        void Send(byte[] data);
        byte[] Receive();
    }
    public interface IServerTransport : ITransport
    {
        IEndpoint Endpoint { get; }

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
        public abstract byte[] Receive();

        protected virtual void OnReceived(byte[] data)
        {
            if (this.Received != null)
                this.Received(this, new ReceivedEventArgs(data));
        }
    }
    public abstract class AClientTransport : IClientTransport
    {
        public event EventHandler<ReceivedEventArgs> Received;

        public IEndpoint Endpoint { get; private set; }

        public AClientTransport(IEndpoint endpoint)
        {
            this.Endpoint = endpoint;
        }

        public abstract void Connect();

        public abstract void Send(byte[] data);
        public abstract byte[] Receive();

        protected virtual void OnReceived(byte[] data)
        {
            if (this.Received != null)
                this.Received(this, new ReceivedEventArgs(data));
        }
    }

    public class ReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; private set; }

        public ReceivedEventArgs(byte[] data)
            : base()
        {
            this.Data = data;
        }
    }
}
