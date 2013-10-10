using Microsoft.WindowsAzure.Storage.Table;

namespace WamsManager.Web.Models
{
    public class CustomerInfo : TableEntity
    {
        public CustomerInfo()
        {

        }

        public CustomerInfo(string invitationId)
        {
            this.PartitionKey = invitationId;
            this.RowKey = invitationId;
        }

        public string InvitationId { get; set; }
        public string SitePrefix { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerOrganization { get; set; }
        public string CustomerEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AzureStorageAccount { get; set; }
        public string AzureStoragePrimaryKey { get; set; }
        public string ProvisionedSiteUrl { get; set; }
        public string SiteCreationMsgTxt { get; set; }
     }
}