using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symvasi.Runtime
{
    public struct SavedEndpoint
    {
        public string Id { get; set; }
        public byte[] Data { get; set; }

        public SavedEndpoint(string id, byte[] data)
            : this()
        {
            this.Id = id;
            this.Data = data;
        }
    }

    public interface IEndpoint
    {
        SavedEndpoint Save();
    }

    public abstract class AEndpoint : IEndpoint
    {
        public override bool Equals(object obj)
        {
            return false;
        }

        public abstract SavedEndpoint Save();
    }
}
