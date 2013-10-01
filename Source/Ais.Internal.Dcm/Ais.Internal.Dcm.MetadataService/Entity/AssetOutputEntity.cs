using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzurePatterns.Entity
{
    public class AssetOutputEntity : TableEntity
    {
        public AssetOutputEntity()
        {

        }

        public AssetOutputEntity(string parentAssetId, string outputAssetId, string encodingName, string friendlyName, string url)
        {
            this.PartitionKey = parentAssetId;
            this.RowKey = outputAssetId;
            this.EncodingName = encodingName;
            this.FriendlyName = friendlyName;
            this.DownloadStreamingUrl = url;
        }

        public string EncodingName { get; set; }
        public string FriendlyName { get; set; }
        public string DownloadStreamingUrl { get; set; }
    }

}
