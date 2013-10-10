using Microsoft.WindowsAzure.Storage.Table;

namespace WamsManager.Web.Models
{
    public class Evaluation : TableEntity
    {
        public string evaluationIdInUse = "0";

        public Evaluation()
        {
        }

        public Evaluation(string invitationId)
        {
            this.PartitionKey = invitationId;
            this.RowKey = invitationId;
        }

        public string EvaluationId { get; set; }
        public string InUse { get; set; }
    }
}