using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symvasi.Runtime.Protocol
{
    public static class ProtocolHelpers
    {
        public static IndefinateTypes ParseIndefinateType(object value)
        {
            IndefinateTypes type;
            if (value is string)
                type = IndefinateTypes.String;
            else if (value is bool)
                type = IndefinateTypes.Boolean;
            else if (value is int)
                type = IndefinateTypes.Integer;
            else if (value is float)
                type = IndefinateTypes.Float;
            else if (value is double)
                type = IndefinateTypes.Double;
            else if (value is byte)
                type = IndefinateTypes.Byte;
            else if (value is Enum)
                type = IndefinateTypes.Enum;
            else if (value is IModel)
                type = IndefinateTypes.Model;
            else if (value is System.Collections.IList)
                type = IndefinateTypes.List;
            else
                throw new Exception("Unsupported any type");

            return type;
        }
    }
}
