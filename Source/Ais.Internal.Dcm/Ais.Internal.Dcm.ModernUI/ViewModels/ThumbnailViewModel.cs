using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.ModernUI.ViewModels
{
    public class ThumbnailViewModel
    {
        public List<Thumbnails> ExistingThumbnails { get; set; }
        public ThumbnailViewModel()
        {
            ExistingThumbnails = new List<Thumbnails>();
            ExistingThumbnails.Add(new Thumbnails { Caption = "1", URL = "http://t1.gstatic.com/images?q=tbn:ANd9GcR8npJUIqq4wcvxE1bRVq3rp0htxD8QohfoRrUOUh-CyGIReyhv" });
            ExistingThumbnails.Add(new Thumbnails { Caption = "2", URL = "http://t3.gstatic.com/images?q=tbn:ANd9GcRaxrH11jfxQ5HItS0nP0aYphTnx7BwctKZwOOb4DdO12ZQKwas" });
            ExistingThumbnails.Add(new Thumbnails { Caption = "3", URL = "http://mediasvckwamscms.blob.core.windows.net/asset-db8cba32-8200-486b-824e-ad1e77453853/WP_20120325_055630Z_00.00.08.jpg" });
        }
    }

    public class Thumbnails
    {
        public string Caption { get; set; }
        public string URL { get; set; }
    }
}
