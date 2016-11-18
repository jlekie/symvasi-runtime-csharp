using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Symvasi.Runtime.Acquisition;

namespace Symvasi.Runtime.Transport
{
    public interface ITransport
    {
        Task<IEndpoint> Listen();
        Task Connect();

        Task Send(byte[] data);
        Task<byte[]> Receive();
    }

    public abstract class ATransport : ITransport
    {
        public ATransport()
        {
        }

        public abstract Task<IEndpoint> Listen();
        public abstract Task Connect();

        public abstract Task Send(byte[] data);
        public abstract Task<byte[]> Receive();
    }
}
