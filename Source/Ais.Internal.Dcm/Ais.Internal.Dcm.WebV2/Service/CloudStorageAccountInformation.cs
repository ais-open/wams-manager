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
