using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzurePatterns.Entity
{
    public class UnCommittedDataEntity : TableEntity
    {
        public UnCommittedDataEntity()
        {

        }

        public UnCommittedDataEntity(string entityId, string type)
        {
            this.MediaServiceEntityType = type;
            this.RowKey = entityId;
            this.UpdateRequired = true;
        }

        public string EntityID { get { return RowKey; } set { this.RowKey = value; } }
        public string Status { get; set; }
        public string AssetID { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool UpdateRequired { get; set; }
        public string MediaServiceEntityType
        {
            get
            { return this.PartitionKey; }
            set
            {
                this.PartitionKey = value;
            }
        }

        public string AdditionalInfo { get; set; }
    }

   
}
