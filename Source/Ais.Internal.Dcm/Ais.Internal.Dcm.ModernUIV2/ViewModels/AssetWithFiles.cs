using System;

namespace Ais.Internal.Dcm.ModernUIV2.ViewModels
{
    public class AssetWithFiles
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public DateTime CreatedDate { get; set; }

        public bool IsUpdated
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Status)) return true;
                return false;
            }
        }

        public string Status { get; set; }
    }
}
