using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetMQ;
using NetMQ.Core;
using NetMQ.Sockets;

namespace Symvasi.Runtime.Transport.ZeroMQ
{
    public class ZeroMQServerTransport : AServerTransport
    {
        public string ConnectionString { get; private set; }

        protected ResponseSocket Socket { get; private set; }

        public ZeroMQServerTransport(string connectionString)
            : base()
        {
            this.ConnectionString = connectionString;
        }

        public override void Listen()
        {
            this.Socket = new ResponseSocket(this.ConnectionString);
        }

        public override void Send(byte[] data)
        {
            this.Socket.SendFrame(data);
        }
        public override byte[] Receive()
        {
            return this.Socket.ReceiveFrameBytes();
        }
    }
    public class ZeroMQRequestServerTransport : AServerTransport
    {
        public string ConnectionString { get; private set; }

        protected RequestSocket Socket { get; private set; }

        private NetMQFrame ClientAddress { get; set; }

        public ZeroMQRequestServerTransport(string connectionString)
            : base()
        {
            this.ConnectionString = connectionString;
        }

        public override void Listen()
        {
            this.Socket = new RequestSocket(this.ConnectionString);

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
            message.Append(data);

            this.Socket.SendMultipartMessage(message);
        }
        public override byte[] Receive()
        {
            var message = this.Socket.ReceiveMultipartMessage();
            if (message.FrameCount != 3)
            {
                throw new Exception("Invalid frame count");
            }

            this.ClientAddress = message[0];

            return message[2].ToByteArray();
        }
    }

    public class ZeroMQClientTransport : AClientTransport
    {
        public string ConnectionString { get; private set; }

        protected RequestSocket Socket { get; private set; }

        public ZeroMQClientTransport(string connectionString)
            : base()
        {
            this.ConnectionString = connectionString;
        }

        public override void Connect()
        {
            this.Socket = new RequestSocket(this.ConnectionString);
        }

        public override void Send(byte[] data)
        {
            this.Socket.SendFrame(data);
        }
        public override byte[] Receive()
        {
            return this.Socket.ReceiveFrameBytes();
        }
    }
}
