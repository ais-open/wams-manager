using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzurePatterns.Entity
{
    public class JobInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

   
}
