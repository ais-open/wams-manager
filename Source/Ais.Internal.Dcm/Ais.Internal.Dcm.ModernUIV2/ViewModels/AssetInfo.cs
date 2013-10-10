using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.ModernUIV2.ViewModels
{
    public class AssetInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DefaultThumbnailUrl { get; set; }
        public string MediaServiceName { get; set; }
        public string Tags { get; set; }
        public string MediaServiceFriendlyName { get; set; }
    }
}
