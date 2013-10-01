using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzurePatterns.Entity
{
    public class TagEntity : TableEntity
    {
        public TagEntity()
        {
            
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}
