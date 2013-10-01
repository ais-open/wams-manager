using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.ModernUIV2.ViewModels
{
    public class GroupedOutputViewModel
    {
        public string FileName { get; set; }
        public string DefaultThumbnailUrl { get; set; }
        public List<VideoOutput> Outputs { get; set; }
        public List<Tag> Tags { get; set; }
        public List<string> Thumbnails { get; set; }

    }
}
