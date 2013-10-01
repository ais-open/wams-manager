using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzurePatterns.Repository;
using Microsoft.WindowsAzure.Storage;
using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.Web.Models;
using Microsoft.WindowsAzure.Storage.Table;
using AzurePatterns.Entity;
namespace Ais.Internal.Dcm.Web.Service
{

    public class CloudStorageAccountInformation : IStorageAccountInformation
    {
        private string _accountName;
        private string _key;
        public CloudStorageAccountInformation()
        {
            var rdr = new System.Configuration.AppSettingsReader();
            this._accountName = (string)rdr.GetValue("MetadataStorageAccountName", typeof(string));
            this._key = (string)rdr.GetValue("MetadataStorageKey", typeof(string));
        }
        public string AccountName
        {
            get
            {
                return this._accountName;
            }
            
        }

        public string Key
        {
            get
            {
                return this._key;
            }
            
        }
    }

}
