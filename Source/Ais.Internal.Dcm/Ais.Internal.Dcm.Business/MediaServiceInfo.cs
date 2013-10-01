using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ais.Internal.Dcm.Business
{
    public class MediaServiceInfo
    {
        public int Id { get; set; }

        public string FriendlyName { get; set; }

        public string AccountName { get; set; }

        public string SecondaryAccountKey { get; set; }

        public string PrimaryAccountKey { get; set; }

        public string ClientKey { get; set; }
    }
}
