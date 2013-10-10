using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzurePatterns.Entity
{
    public class AssetFileEntity : TableEntity
    {
        public AssetFileEntity()
        {
            
        }

        public AssetFileEntity(string parentAssetId, string assetId)
        {
            this.PartitionKey = parentAssetId;
            this.RowKey = assetId;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string DefaultThumnailUrl { get; set; }
        public long ContentFileSize { get; set; }
        public string ParentAssetId { get; set; }
        public string MimeType { get; set; }
        public bool IsPrimary { get; set; }
        public string Tags { get; set; }
        //public DateTime LastModified { get; set; }
        //public DateTime Created { get; set; }
    }


}
