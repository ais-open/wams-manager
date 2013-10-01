using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.ModernUIV2.ViewModels
{
    public class MediaServiceInfo
    {
        public int Id { get; set; }

        public string MediaServiceFriendlyName { get; set; }

        public string AccountName { get; set; }

        public string SecondaryAccountKey { get; set; }

        public string PrimaryAccountKey { get; set; }

        public string ClientKey { get; set; }
    }
}
