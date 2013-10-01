using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzurePatterns.Entity
{

    public class AssetInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DefaultThumbnailUrl { get; set; }
        public string MediaServiceName { get; set; }
        public string Tags { get; set; }
    }
}
