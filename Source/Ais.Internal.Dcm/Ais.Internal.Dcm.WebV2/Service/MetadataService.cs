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

namespace Ais.Internal.Dcm.Web.Service
{
    public class MetaDataService : IMetadataService
    {
        private Microsoft.WindowsAzure.Storage.CloudStorageAccount account;
        public MetaDataService(IStorageAccountInformation info)
        {
            account = new Microsoft.WindowsAzure.Storage.CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(info.AccountName, info.Key), true);
        }
        public AssetFileRepository GetAssetFileRepository()
        {
            return new AssetFileRepository(account);
        }

        public AssetOutputRepository GetAssetOutputRepository()
        {
            return new AssetOutputRepository(account);
        }

        public AssetRepository GetAssetRepository()
        {
            return new AssetRepository(account);
        }

        public AssetThumbnailRepository GetAssetThumbnailRepository()
        {
            return new AssetThumbnailRepository(account);
        }

        public EncodingTypeRepository GetEncodingTypeRepository()
        {
            return new EncodingTypeRepository(account);
        }

        public MediaServiceRepository GetMediaServiceRepository()
        {
            return new MediaServiceRepository(account);
        }

        public UnCommittedDataRepository GetUnCommittedDataRepository()
        {
            return new UnCommittedDataRepository(account);
        }

        public TagRepository GetTagRepository()
        {
            return new TagRepository(account);
        }
    }

}
