using AzurePatterns.Repository;

namespace Ais.Internal.Dcm.Web.Service
{
    public class MetaDataService : IMetadataService
    {
        string clientId
        {
            get
            {
                var rdr = new System.Configuration.AppSettingsReader();
                string ID = (string)rdr.GetValue("customer_append", typeof(string));
                string companyName = ID.Split('@')[1].Split('.')[0];
                return companyName;
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
