using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symvasi.Runtime
{
    public interface IEndpoint
    {
    }
    public interface IServerEndpoint : IEndpoint
    {
        byte[] Save();
    }
    public interface IClientEndpoint : IEndpoint
    {
    }
}
