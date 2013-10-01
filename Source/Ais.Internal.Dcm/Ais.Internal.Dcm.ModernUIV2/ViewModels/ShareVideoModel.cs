using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.ModernUIV2.ViewModels
{
    class ShareVideoModel
    {
        public  List<VideoOutput> EncodingTypes { get; set; }
        public string SelectedVideoURL { get; set; }
        public string DefaultThumbnailURL { get; set; }
    }
}
