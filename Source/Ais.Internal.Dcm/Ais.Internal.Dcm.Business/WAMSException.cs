using System;
using System.Diagnostics.CodeAnalysis;

namespace Ais.Internal.Dcm.Business
{
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "No serialization required for this exception")]
    public class WAMSException : Exception
    {
        public string WAMSMessage { get; set; }
        public string Detail { get; set; }

        public WAMSException(string message):base(message)
        {
            this.WAMSMessage = message;
        }

        public WAMSException() :base()
        {

        }
    }
}
