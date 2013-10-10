using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzurePatterns.Entity
{
    public class MediaServiceEntity : TableEntity
    {
        public MediaServiceEntity()
        {
        }

        public MediaServiceEntity(string clientID, string mediaServiceName)
        {
            this.PartitionKey = clientID;
            this.RowKey = mediaServiceName;
        }
        public string FriendlyName { get; set; }

        public string AccessKey { get; set; }

        //public string ID { get; set; }
    }
}
