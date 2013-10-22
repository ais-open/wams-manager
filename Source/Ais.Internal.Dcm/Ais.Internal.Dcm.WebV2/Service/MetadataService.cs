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
using Microsoft.WindowsAzure;
using System.Configuration;

namespace Ais.Internal.Dcm.Web.Service
{
    public class MetaDataService : IMetadataService
    {
        string clientId
        {
            get
            {
                var rdr = new System.Configuration.AppSettingsReader();
                return (string)rdr.GetValue("customer_append", typeof(string));
            }
        }

        private Microsoft.WindowsAzure.Storage.CloudStorageAccount account;
        public MetaDataService(IStorageAccountInformation info)
        {
            account = new Microsoft.WindowsAzure.Storage.CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(info.AccountName, info.Key), true);
        }
        public AssetFileRepository GetAssetFileRepository()
        {
            return new AssetFileRepository(account, clientId);
        }

        public AssetOutputRepository GetAssetOutputRepository()
        {
            return new AssetOutputRepository(account,clientId);
        }

        public AssetRepository GetAssetRepository()
        {
            return new AssetRepository(account,clientId);
        }

        public AssetThumbnailRepository GetAssetThumbnailRepository()
        {
            return new AssetThumbnailRepository(account,clientId);
        }

        public EncodingTypeRepository GetEncodingTypeRepository()
        {
            return new EncodingTypeRepository(account,clientId);
        }

        public MediaServiceRepository GetMediaServiceRepository()
        {
            return new MediaServiceRepository(account,clientId);
        }

        public UnCommittedDataRepository GetUnCommittedDataRepository()
        {
            return new UnCommittedDataRepository(account,clientId);
        }

        public TagRepository GetTagRepository()
        {
            return new TagRepository(account,clientId);
        }
    }

}
