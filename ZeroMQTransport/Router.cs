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
    //internal struct KnownBackend
    //{
    //    public DateTime LastRegistered { get; set; }
    //    public NetMQFrame Address { get; set; }

    //    public KnownBackend(NetMQFrame address)
    //        : this()
    //    {
    //        this.Address = address;
    //        this.LastRegistered = DateTime.Now;
    //    }
    //}

    //public class Router
    //{
    //    public IZeroMQEndpoint FrontendEndpoint { get; private set; }
    //    public IZeroMQEndpoint BackendEndpoint { get; private set; }

    //    private Task HandlerTask { get; set; }

    //    private System.Collections.Concurrent.ConcurrentQueue<KnownBackend> KnownBackends { get; set; }

    //    public Router(IZeroMQEndpoint frontendEndpoint, IZeroMQEndpoint backendEndpoint)
    //    {
    //        this.KnownBackends = new System.Collections.Concurrent.ConcurrentQueue<KnownBackend>();

    //        this.FrontendEndpoint = frontendEndpoint;
    //        this.BackendEndpoint = backendEndpoint;
    //    }

    //    public void Start()
    //    {
    //        this.HandlerTask = Task.Factory.StartNew(() => this.Handler(), TaskCreationOptions.LongRunning);
    //    }
    //    public void Stop()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    private void Handler()
    //    {
    //        var frontendConnectionString = this.FrontendEndpoint.ToServerConnectionString();
    //        var backendConnectionString = this.BackendEndpoint.ToServerConnectionString();

    //        using (var frontendSocket = new RouterSocket(frontendConnectionString))
    //        using (var backendSocket = new RouterSocket(backendConnectionString))
    //        using (var poller = new NetMQPoller() { frontendSocket, backendSocket })
    //        {
    //            frontendSocket.ReceiveReady += (s, a) =>
    //            {
    //                try
    //                {
    //                    var message = a.Socket.ReceiveMultipartMessage(3);

    //                    var frontendAddress = message[0];
    //                    var requestData = message[2];

    //                    KnownBackend backend;
    //                    this.KnownBackends.TryDequeue(out backend);

    //                    var backendMessage = new NetMQMessage();
    //                    backendMessage.Append(backend.Address);
    //                    backendMessage.AppendEmptyFrame();
    //                    for (int frameIdx = 0; frameIdx < message.FrameCount; frameIdx++)
    //                        backendMessage.Append(message[frameIdx]);

    //                    backendSocket.SendMultipartMessage(backendMessage);

    //                    this.KnownBackends.Enqueue(backend);
    //                }
    //                catch (Exception ex)
    //                {
    //                    Console.WriteLine("Error: " + ex.Message);
    //                }
    //            };

    //            backendSocket.ReceiveReady += (s, a) =>
    //            {
    //                try
    //                {
    //                    var message = a.Socket.ReceiveMultipartMessage(3);
    //                    if (message.FrameCount == 3)
    //                    {
    //                        var address = message[0];
    //                        var data = message[2];

    //                        this.KnownBackends.Enqueue(new KnownBackend(address));

    //                        var responseMessage = new NetMQMessage();
    //                        responseMessage.Append(address);
    //                        responseMessage.AppendEmptyFrame();
    //                        responseMessage.AppendEmptyFrame();

    //                        Console.WriteLine("Sending registration acknowledgement...");
    //                        a.Socket.SendMultipartMessage(responseMessage);
    //                        Console.WriteLine("Waiting for all clear...");
    //                        a.Socket.SkipMultipartMessage();
    //                    }
    //                    else if (message.FrameCount >= 5)
    //                    {
    //                        var address = message[0];
    //                        var frontendAddress = message[2];

    //                        var responseMessage = new NetMQMessage();
    //                        responseMessage.Append(frontendAddress);
    //                        responseMessage.AppendEmptyFrame();
    //                        for (int frameIdx = 4; frameIdx < message.FrameCount; frameIdx++)
    //                            responseMessage.Append(message[frameIdx]);

    //                        frontendSocket.SendMultipartMessage(responseMessage);
    //                    }
    //                    else
    //                    {
    //                        throw new Exception("Invalid frame count");
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    Console.WriteLine("Error: " + ex.Message);
    //                }
    //            };

    //            poller.Run();
    //        }
    //    }
    //}

    //public class DiscoveryRouter
    //{
    //    public IZeroMQEndpoint FrontendEndpoint { get; private set; }
    //    public IDiscoverer BackendDiscoverer { get; private set; }

    //    private Task HandlerTask { get; set; }

    //    public DiscoveryRouter(IZeroMQEndpoint frontendEndpoint, IDiscoverer backendDiscoverer)
    //    {
    //        this.FrontendEndpoint = frontendEndpoint;
    //        this.BackendDiscoverer = backendDiscoverer;
    //    }

    //    public void Start()
    //    {
    //        this.HandlerTask = Task.Factory.StartNew(() => this.Handler(), TaskCreationOptions.LongRunning);
    //    }
    //    public void Stop()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    private void Handler()
    //    {
    //        var frontendConnectionString = this.FrontendEndpoint.ToServerConnectionString();

    //        using (var frontendSocket = new RouterSocket(frontendConnectionString))
    //        using (var poller = new NetMQPoller() { frontendSocket })
    //        {
    //            frontendSocket.ReceiveReady += (s, a) =>
    //            {
    //                try
    //                {
    //                    var message = a.Socket.ReceiveMultipartMessage(5);

    //                    var frontendAddress = message[0];

    //                    Task.Factory.StartNew(() =>
    //                    {
    //                        var backendEndpoint = this.BackendDiscoverer.GetEndpoint();

    //                        using (var backendSocket = new RequestSocket(backendEndpoint.ToClientConnectionString()))
    //                        {
    //                            var requestMessage = new NetMQMessage();
    //                            for (int frameIdx = 2; frameIdx < message.FrameCount; frameIdx++)
    //                                requestMessage.Append(message[frameIdx]);
                                
    //                            backendSocket.SendMultipartMessage(requestMessage);
    //                            var backendMessage = backendSocket.ReceiveMultipartMessage(3);

    //                            return backendMessage;
    //                        }
    //                    }, TaskCreationOptions.LongRunning).ContinueWith((task) =>
    //                    {
    //                        if (!task.IsFaulted)
    //                        {
    //                            var backendMessage = task.Result;

    //                            var responseMessage = new NetMQMessage();
    //                            responseMessage.Append(frontendAddress);
    //                            responseMessage.AppendEmptyFrame();
    //                            for (int frameIdx = 0; frameIdx < backendMessage.FrameCount; frameIdx++)
    //                                responseMessage.Append(backendMessage[frameIdx]);

    //                            frontendSocket.SendMultipartMessage(responseMessage);
    //                        }
    //                        else
    //                        {
    //                            Console.WriteLine("Warning: " + task.Exception.InnerException.Message);

    //                            var responseMessage = new NetMQMessage();
    //                            responseMessage.Append(frontendAddress);
    //                            responseMessage.AppendEmptyFrame();
    //                            responseMessage.Append(1);
    //                            responseMessage.AppendEmptyFrame();
    //                            responseMessage.Append(task.Exception.InnerException.Message, Encoding.UTF8);

    //                            frontendSocket.SendMultipartMessage(responseMessage);
    //                        }
    //                    }, TaskScheduler.FromCurrentSynchronizationContext());
    //                }
    //                catch (Exception ex)
    //                {
    //                    Console.WriteLine("Error: " + ex.Message);
    //                }
    //            };

    //            poller.Run();
    //        }
    //    }
    //}
}
