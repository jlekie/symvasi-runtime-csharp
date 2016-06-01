﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetMQ;
using NetMQ.Core;
using NetMQ.Sockets;

namespace Symvasi.Runtime.Transport.ZeroMQ
{
    public class Router
    {
        public string FrontendConnectionString { get; private set; }
        public string BackendConnectionString { get; private set; }

        private Task HandlerTask { get; set; }

        private Queue<NetMQFrame> KnownBackends { get; set; }

        public Router(string frontendConnectionString, string backendConnectionString)
        {
            this.KnownBackends = new Queue<NetMQFrame>();

            this.FrontendConnectionString = frontendConnectionString;
            this.BackendConnectionString = backendConnectionString;
        }

        public void Start()
        {
            this.HandlerTask = Task.Factory.StartNew(() => this.Handler(), TaskCreationOptions.LongRunning);
        }
        public void Stop()
        {
        }

        private void Handler()
        {
            using (var frontendSocket = new RouterSocket(this.FrontendConnectionString))
            using (var backendSocket = new RouterSocket(this.BackendConnectionString))
            using (var poller = new NetMQPoller() { frontendSocket, backendSocket })
            {
                frontendSocket.ReceiveReady += (s, a) =>
                {
                    try
                    {
                        var message = a.Socket.ReceiveMultipartMessage();
                        if (message.FrameCount != 3)
                        {
                            throw new Exception("Invalid frame count");
                        }

                        var frontendAddress = message[0];
                        var requestData = message[2];

                        var backendAddress = this.KnownBackends.Dequeue();
                        this.KnownBackends.Enqueue(backendAddress);

                        var backendMessage = new NetMQMessage();
                        backendMessage.Append(backendAddress);
                        backendMessage.AppendEmptyFrame();
                        backendMessage.Append(frontendAddress);
                        backendMessage.AppendEmptyFrame();
                        backendMessage.Append(requestData);

                        backendSocket.SendMultipartMessage(backendMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                };

                backendSocket.ReceiveReady += (s, a) =>
                {
                    try
                    {
                        var message = a.Socket.ReceiveMultipartMessage();
                        if (message.FrameCount == 3)
                        {
                            var address = message[0];
                            var data = message[2];

                            this.KnownBackends.Enqueue(address);

                            var responseMessage = new NetMQMessage();
                            responseMessage.Append(address);
                            responseMessage.AppendEmptyFrame();
                            responseMessage.AppendEmptyFrame();

                            Console.WriteLine("Sending registration acknowledgement...");
                            a.Socket.SendMultipartMessage(responseMessage);
                            Console.WriteLine("Waiting for all clear...");
                            a.Socket.SkipMultipartMessage();
                        }
                        else if (message.FrameCount == 5)
                        {
                            var address = message[0];
                            var frontendAddress = message[2];
                            var data = message[4];

                            var responseMessage = new NetMQMessage();
                            responseMessage.Append(frontendAddress);
                            responseMessage.AppendEmptyFrame();
                            responseMessage.Append(data);

                            frontendSocket.SendMultipartMessage(responseMessage);
                        }
                        else
                        {
                            throw new Exception("Invalid frame count");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                };

                poller.Run();
            }
        }
    }
}