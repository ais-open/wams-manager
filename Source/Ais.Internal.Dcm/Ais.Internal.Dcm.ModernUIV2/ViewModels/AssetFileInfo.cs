using System;

namespace Ais.Internal.Dcm.ModernUIV2.ViewModels
{
    public class AssetFileInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DefaultThumbnailUrl { get; set; }
        public long ContentFileSize { get; set; }
        public string ParentAssetId { get; set; }
        public string MimeType { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime Created { get; set; }
        public string Tags { get; set; }
    }
}
