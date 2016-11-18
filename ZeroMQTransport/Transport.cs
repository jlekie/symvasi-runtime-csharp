using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetMQ;
using NetMQ.Core;
using NetMQ.Sockets;

using Symvasi.Runtime.Acquisition;

namespace Symvasi.Runtime.Transport.ZeroMQ
{
    public interface IZeroMQReqRepTransport : ITransport
    {
        //IZeroMQEndpoint Endpoint { get; }
        //string InterfaceName { get; set; }
    }

    public class ZeroMQReqRepTransport : ATransport, IZeroMQReqRepTransport
    {
        public Func<IZeroMQEndpoint> ServerEndpointFactory { get; private set; }

        private NetMQSocket Socket { get; set; }

        //public new IZeroMQEndpoint Endpoint
        //{
        //    get
        //    {
        //        return (IZeroMQEndpoint)base.Endpoint;
        //    }
        //}
        //public IZeroMQEndpoint Endpoint { get; private set; }

        public ZeroMQReqRepTransport(Func<IZeroMQEndpoint> serverEndpointFactory)
            : base()
        {
            this.ServerEndpointFactory = serverEndpointFactory;
        }

        public override Task<IEndpoint> Listen()
        {
            return Task.Run<IEndpoint>(() =>
            {
                var zmqEndpoint = this.ServerEndpointFactory();
                var connectionString = zmqEndpoint.ToServerConnectionString();

                this.Socket = new ResponseSocket();
                this.Socket.Bind(connectionString);

                return zmqEndpoint;
            });
        }
        public override Task Connect()
        {
            return Task.Run(() =>
            {
                var zmqEndpoint = this.ServerEndpointFactory();
                if (zmqEndpoint == null)
                    throw new Exception("Invalid endpoint");

                var connectionString = zmqEndpoint.ToClientConnectionString();

                this.Socket = new RequestSocket();
                this.Socket.Connect(connectionString);
            });
        }

        public override Task Send(byte[] data)
        {
            return Task.Run(() =>
            {
                var message = new NetMQMessage();
                message.Append(0);
                message.AppendEmptyFrame();
                message.Append(data);

                this.Socket.SendMultipartMessage(message);
            });
        }
        public override Task<byte[]> Receive()
        {
            return Task.Run(() =>
            {
                var message = this.Socket.ReceiveMultipartMessage(3);

                var signal = message[0].ConvertToInt32();
                if (signal != 0)
                {
                    var errorMessage = message[2].ConvertToString(Encoding.UTF8);
                    throw new Exception(string.Format("Server responded with ({0}) '{1}'", signal, errorMessage));
                }

                return message[2].ToByteArray();
            });
        }
    }

    //public class ZeroMQServerTransport : AServerTransport
    //{
    //    public IZeroMQEndpoint ZeroMQEndpoint { get; private set; }

    //    protected ResponseSocket Socket { get; private set; }

    //    public ZeroMQServerTransport(IZeroMQEndpoint endpoint)
    //        : base(endpoint)
    //    {
    //        this.ZeroMQEndpoint = endpoint;
    //    }

    //    public override void Listen()
    //    {
    //        var connectionString = this.ZeroMQEndpoint.ToServerConnectionString();

    //        this.Socket = new ResponseSocket(connectionString);
    //    }

    //    public override void Send(byte[] data)
    //    {
    //        var message = new NetMQMessage();
    //        message.Append(0);
    //        message.AppendEmptyFrame();
    //        message.Append(data);

    //        this.Socket.SendMultipartMessage(message);
    //    }
    //    public override void Send(byte[] data, Guid schedulerId)
    //    {
    //        this.Send(data);
    //    }

    //    public override byte[] Receive()
    //    {
    //        var message = this.Socket.ReceiveMultipartMessage(3);

    //        var signal = message[0].ConvertToInt32();
    //        if (signal != 0)
    //        {
    //            var errorMessage = message[2].ConvertToString(Encoding.UTF8);
    //            throw new Exception(string.Format("Server responded with ({0}) '{1}'", signal, errorMessage));
    //        }

    //        return message[2].ToByteArray();
    //    }
    //    public override byte[] Receive(Guid schedulerId)
    //    {
    //        return this.Receive();
    //    }
    //}
    //public class ZeroMQRequestServerTransport : AServerTransport
    //{
    //    public IZeroMQEndpoint ZeroMQEndpoint { get; private set; }

    //    protected RequestSocket Socket { get; private set; }

    //    private NetMQFrame ClientAddress { get; set; }

    //    public ZeroMQRequestServerTransport(IZeroMQEndpoint endpoint)
    //        : base(endpoint)
    //    {
    //        this.ZeroMQEndpoint = endpoint;
    //    }

    //    public override void Listen()
    //    {
    //        var connectionString = this.ZeroMQEndpoint.ToClientConnectionString();

    //        this.Socket = new RequestSocket(connectionString);

    //        Console.WriteLine("Sending registration request...");
    //        this.Socket.SendFrameEmpty();
    //        Console.WriteLine("Waiting for registration acknowledgement...");
    //        this.Socket.SkipMultipartMessage();
    //        Console.WriteLine("Sending all clear...");
    //        this.Socket.SendFrameEmpty();
    //    }

    //    public override void Send(byte[] data)
    //    {
    //        var message = new NetMQMessage();
    //        message.Append(this.ClientAddress);
    //        message.AppendEmptyFrame();
    //        message.Append(0);
    //        message.AppendEmptyFrame();
    //        message.Append(data);

    //        this.Socket.SendMultipartMessage(message);
    //    }
    //    public override void Send(byte[] data, Guid schedulerId)
    //    {
    //        this.Send(data);
    //    }

    //    public override byte[] Receive()
    //    {
    //        var message = this.Socket.ReceiveMultipartMessage(5);

    //        this.ClientAddress = message[0];

    //        var signal = message[2].ConvertToInt32();
    //        if (signal != 0)
    //        {
    //            var errorMessage = message[4].ConvertToString(Encoding.UTF8);
    //            throw new Exception(string.Format("Server responded with ({0}) '{1}'", signal, errorMessage));
    //        }

    //        return message[4].ToByteArray();
    //    }
    //    public override byte[] Receive(Guid schedulerId)
    //    {
    //        return this.Receive();
    //    }
    //}
    //public class ZeroMQRouterServerTransport : AServerTransport
    //{
    //    public IZeroMQEndpoint ZeroMQEndpoint { get; private set; }

    //    protected RouterSocket Socket { get; private set; }
    //    protected NetMQPoller Poller { get; private set; }

    //    protected Dictionary<Guid, NetMQFrame> AddressRegistry { get; private set; }

    //    private Task ServerTask { get; set; }

    //    public ZeroMQRouterServerTransport(IZeroMQEndpoint endpoint)
    //        : base(endpoint)
    //    {
    //        this.ZeroMQEndpoint = endpoint;

    //        this.AddressRegistry = new Dictionary<Guid, NetMQFrame>();
    //    }

    //    public override void Listen()
    //    {
    //        var connectionString = this.ZeroMQEndpoint.ToServerConnectionString();

    //        this.Socket = new RouterSocket(connectionString);
    //        this.Socket.ReceiveReady += (sender, e) =>
    //        {
    //            this.OnReceived();
    //        };

    //        this.Poller = new NetMQPoller() { this.Socket };

    //        this.ServerTask = Task.Factory.StartNew(() => this.ServerHandler(), TaskCreationOptions.LongRunning);
    //    }

    //    public override void Send(byte[] data)
    //    {
    //        throw new Exception("Router transport requires a scheduler ID");
    //    }
    //    public override void Send(byte[] data, Guid schedulerId)
    //    {
    //        NetMQFrame address;
    //        if (!this.AddressRegistry.TryGetValue(schedulerId, out address))
    //        {
    //            throw new Exception(string.Format("No address registered for schedule ID '{0}'", schedulerId));
    //        }
    //        this.AddressRegistry.Remove(schedulerId);

    //        var message = new NetMQMessage();
    //        message.Append(address);
    //        message.AppendEmptyFrame();
    //        message.Append(0);
    //        message.AppendEmptyFrame();
    //        message.Append(data);

    //        this.Socket.SendMultipartMessage(message);
    //    }

    //    public override byte[] Receive()
    //    {
    //        throw new Exception("Router transport requires a scheduler ID");
    //    }
    //    public override byte[] Receive(Guid schedulerId)
    //    {
    //        var message = this.Socket.ReceiveMultipartMessage(5);

    //        var address = message[0];
    //        if (this.AddressRegistry.ContainsKey(schedulerId))
    //        {
    //            throw new Exception(string.Format("Address already registered for schedule ID '{0}'", schedulerId));
    //        }
    //        this.AddressRegistry.Add(schedulerId, address);

    //        var signal = message[2].ConvertToInt32();
    //        if (signal != 0)
    //        {
    //            var errorMessage = message[4].ConvertToString(Encoding.UTF8);
    //            throw new Exception(string.Format("Server responded with ({0}) '{1}'", signal, errorMessage));
    //        }

    //        return message[4].ToByteArray();
    //    }

    //    private void ServerHandler()
    //    {
    //        this.Poller.Run();
    //    }
    //}

    //public class ZeroMQClientTransport : AClientTransport
    //{
    //    public IZeroMQEndpoint ZeroMQEndpoint { get; private set; }

    //    protected RequestSocket Socket { get; private set; }

    //    public ZeroMQClientTransport(IZeroMQEndpoint endpoint)
    //        : base(endpoint)
    //    {
    //        this.ZeroMQEndpoint = endpoint;
    //    }

    //    public override void Connect()
    //    {
    //        var connectionString = this.ZeroMQEndpoint.ToClientConnectionString();

    //        this.Socket = new RequestSocket(connectionString);
    //    }

    //    public override void Send(byte[] data)
    //    {
    //        var message = new NetMQMessage();
    //        message.Append(0);
    //        message.AppendEmptyFrame();
    //        message.Append(data);

    //        this.Socket.SendMultipartMessage(message);
    //    }
    //    public override byte[] Receive()
    //    {
    //        var message = this.Socket.ReceiveMultipartMessage(3);

    //        var signal = message[0].ConvertToInt32();
    //        if (signal != 0)
    //        {
    //            var errorMessage = message[2].ConvertToString(Encoding.UTF8);
    //            throw new Exception(string.Format("Server responded with ({0}) '{1}'", signal, errorMessage));
    //        }

    //        return message[2].ToByteArray();
    //    }
    //}
}
