using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.ModernUIV2.ViewModels
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class WAMSException : Exception
    {
        public string WAMSMessage { get; set; }
        public string Detail { get; set; }

        public WAMSException(string message)
            : base(message)
        {
            this.WAMSMessage = message;
        }

        public WAMSException()
            : base()
        {

        }
    }
}
