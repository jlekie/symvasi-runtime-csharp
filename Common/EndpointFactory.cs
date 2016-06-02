using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symvasi.Runtime
{
    public interface IEndpointFactory<TEndpoint> where TEndpoint : IEndpoint
    {
        TEndpoint Load(byte[] data);
    }
}
