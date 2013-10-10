using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzurePatterns.Entity
{
    public class EncodingTypeEntity :TableEntity
    {
       
            public EncodingTypeEntity()
            {
            }

            public EncodingTypeEntity(string encodingTechnicalName, string encodingFriendlyName)
            {
                this.PartitionKey = "Universal";
                this.RowKey = encodingTechnicalName;
                this.FriendlyName = encodingFriendlyName;
            }
            public string FriendlyName { get; set; }
    }
}
