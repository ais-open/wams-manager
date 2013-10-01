using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzurePatterns.Entity
{
    public class AssetThumbnailEntity : TableEntity
    {
        public AssetThumbnailEntity()
        {

        }

        public AssetThumbnailEntity(string assetId, string thumbnailId, string assetFileId, string friendlyName, string url)
        {
            this.PartitionKey = assetId;
            this.RowKey = thumbnailId;
            this.FriendlyName = friendlyName;
            this.DownloadStreamingUrl = url;
        }

        public string AssetFileId { get; set; }
        public string FriendlyName { get; set; }
        public string DownloadStreamingUrl { get; set; }
    }
}
