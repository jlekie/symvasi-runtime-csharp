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
    public class ZeroMQServerTransport : AServerTransport
    {
        public IZeroMQEndpoint ZeroMQEndpoint { get; private set; }

        protected ResponseSocket Socket { get; private set; }

        public ZeroMQServerTransport(IZeroMQEndpoint endpoint)
            : base(endpoint)
        {
            this.ZeroMQEndpoint = endpoint;
        }

        public override void Listen()
        {
            var connectionString = this.ZeroMQEndpoint.ToServerConnectionString();

            this.Socket = new ResponseSocket(connectionString);
        }

        public override void Send(byte[] data)
        {
            var message = new NetMQMessage();
            message.Append(0);
            message.AppendEmptyFrame();
            message.Append(data);

            this.Socket.SendMultipartMessage(message);
        }
        public override byte[] Receive()
        {
            var message = this.Socket.ReceiveMultipartMessage(3);

            var signal = message[0].ConvertToInt32();
            if (signal != 0)
            {
                var errorMessage = message[2].ConvertToString(Encoding.UTF8);
                throw new Exception(string.Format("Server responded with ({0}) '{1}'", signal, errorMessage));
            }

            return message[2].ToByteArray();
        }
    }
    public class ZeroMQRequestServerTransport : AServerTransport
    {
        public IZeroMQEndpoint ZeroMQEndpoint { get; private set; }

        protected RequestSocket Socket { get; private set; }

        private NetMQFrame ClientAddress { get; set; }

        public ZeroMQRequestServerTransport(IZeroMQEndpoint endpoint)
            : base(endpoint)
        {
            this.ZeroMQEndpoint = endpoint;
        }

        public override void Listen()
        {
            var connectionString = this.ZeroMQEndpoint.ToClientConnectionString();

            this.Socket = new RequestSocket(connectionString);

            Console.WriteLine("Sending registration request...");
            this.Socket.SendFrameEmpty();
            Console.WriteLine("Waiting for registration acknowledgement...");
            this.Socket.SkipMultipartMessage();
            Console.WriteLine("Sending all clear...");
            this.Socket.SendFrameEmpty();
        }

        public override void Send(byte[] data)
        {
            var message = new NetMQMessage();
            message.Append(this.ClientAddress);
            message.AppendEmptyFrame();
            message.Append(0);
            message.AppendEmptyFrame();
            message.Append(data);

            this.Socket.SendMultipartMessage(message);
        }
        public override byte[] Receive()
        {
            var message = this.Socket.ReceiveMultipartMessage(5);

            this.ClientAddress = message[0];

            var signal = message[2].ConvertToInt32();
            if (signal != 0)
            {
                var errorMessage = message[4].ConvertToString(Encoding.UTF8);
                throw new Exception(string.Format("Server responded with ({0}) '{1}'", signal, errorMessage));
            }

            return message[4].ToByteArray();
        }
    }

    public class ZeroMQClientTransport : AClientTransport
    {
        public IZeroMQEndpoint ZeroMQEndpoint { get; private set; }

        protected RequestSocket Socket { get; private set; }

        public ZeroMQClientTransport(IZeroMQEndpoint endpoint)
            : base(endpoint)
        {
            this.ZeroMQEndpoint = endpoint;
        }

        public override void Connect()
        {
            var connectionString = this.ZeroMQEndpoint.ToClientConnectionString();

            this.Socket = new RequestSocket(connectionString);
        }

        public override void Send(byte[] data)
        {
            var message = new NetMQMessage();
            message.Append(0);
            message.AppendEmptyFrame();
            message.Append(data);

            this.Socket.SendMultipartMessage(message);
        }
        public override byte[] Receive()
        {
            var message = this.Socket.ReceiveMultipartMessage(3);

            var signal = message[0].ConvertToInt32();
            if (signal != 0)
            {
                var errorMessage = message[2].ConvertToString(Encoding.UTF8);
                throw new Exception(string.Format("Server responded with ({0}) '{1}'", signal, errorMessage));
            }

            return message[2].ToByteArray();
        }
    }
}
