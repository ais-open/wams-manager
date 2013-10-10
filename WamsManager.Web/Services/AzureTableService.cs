using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using WamsManager.Web.Models;
using System.Configuration;

namespace WamsManager.Web
{
    public class AzureTableService
    {
        private CloudStorageAccount storageAccount;
        private CloudTableClient tableClient;
        private CloudTable evaluationTable;
        private CloudTable customerInfoTable;

        public AzureTableService()
        {
            //storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConfigurationString"));
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureStorageConnection"].ConnectionString);
            tableClient = storageAccount.CreateCloudTableClient();

            evaluationTable = tableClient.GetTableReference("Evaluation");
            evaluationTable.CreateIfNotExists();

            customerInfoTable = tableClient.GetTableReference("CustomerInfo");
            customerInfoTable.CreateIfNotExists();
        }

        public bool IsEvaluationIdValid(string evaluationId)
        {
            TableQuery<Evaluation> rangeQuery = new TableQuery<Evaluation>().Where(
                TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("EvaluationId", QueryComparisons.Equal, evaluationId),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("InUse", QueryComparisons.Equal, "0")));

            List<Evaluation> existingEntities = evaluationTable.ExecuteQuery(rangeQuery).ToList();

            return existingEntities.Count > 0;
        }

        public void UpdateEvaluationId(string evaluationId)
        {
            TableQuery<Evaluation> rangeQuery = new TableQuery<Evaluation>().Where(
                TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("EvaluationId", QueryComparisons.Equal, evaluationId),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("InUse", QueryComparisons.Equal, "0")));

            List<Evaluation> existingEntities = evaluationTable.ExecuteQuery(rangeQuery).ToList();

            if (existingEntities.Count > 0) 
            {
                existingEntities[0].InUse = "1";
                evaluationTable.Execute(TableOperation.Merge(existingEntities[0]));
            }
        }

        public void InsertCustomerInfoTable(CustomerInfo customerInfo)
        {
            var custInfo = new CustomerInfo(customerInfo.InvitationId)
                               {
                                   InvitationId = customerInfo.InvitationId,
                                   CustomerFirstName = customerInfo.CustomerFirstName,
                                   CustomerLastName = customerInfo.CustomerLastName,
                                   SitePrefix = customerInfo.SitePrefix,
                                   CustomerOrganization = customerInfo.CustomerOrganization,
                                   CustomerEmail = customerInfo.CustomerEmail,
                                   Username = customerInfo.Username,
                                   Password = customerInfo.Password,
                                   AzureStorageAccount = customerInfo.AzureStorageAccount,
                                   AzureStoragePrimaryKey = customerInfo.AzureStoragePrimaryKey
                               };

            customerInfoTable.Execute(TableOperation.Insert(custInfo));
        }

        public void UpdateCustomerInfoTableWithSiteInfo(string partitionKey, string rowKey, string siteUrl)
        {
            TableResult res = customerInfoTable.Execute(TableOperation.Retrieve<CustomerInfo>(partitionKey, rowKey));
            var updateCustInfo = (CustomerInfo)res.Result;
            updateCustInfo.ProvisionedSiteUrl = siteUrl;
            customerInfoTable.Execute(TableOperation.Merge(updateCustInfo));
        }

        public bool IsAlreadySiteProvisionedForInvitationId(string evaluationId)
        {
            TableQuery<CustomerInfo> rangeQuery = new TableQuery<CustomerInfo>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("InvitationId",QueryComparisons.Equal, evaluationId),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("ProvisionedSiteUrl", QueryComparisons.NotEqual, null)));

            List<CustomerInfo> existingEntities = evaluationTable.ExecuteQuery(rangeQuery).ToList();

            return existingEntities.Count > 0;
        }

        public string GetProvisionedSiteUrl(string evaluationId)
        {
            TableResult res = customerInfoTable.Execute(TableOperation.Retrieve<CustomerInfo>(evaluationId, evaluationId));
            var updateCustInfo = (CustomerInfo)res.Result;

            return updateCustInfo.ProvisionedSiteUrl;
        }

        #region OldCode

        //public void InsertSeedData()
        //{
        //    TableQuery<Evaluation> rangeQuery = new TableQuery<Evaluation>().Where(
        //      TableQuery.GenerateFilterCondition("EvaluationId", QueryComparisons.Equal, null).Count(0));

        //    List<Evaluation> existingEntities = evaluationTable.ExecuteQuery(rangeQuery).ToList();

        //    if (existingEntities.Count > 0)
        //    {
        //        var batchDeleteOperation = new TableBatchOperation();

        //        foreach (Evaluation entity in existingEntities)
        //        {
        //            batchDeleteOperation.Delete(entity);
        //        }

        //        evaluationTable.ExecuteBatch(batchDeleteOperation);
        //    }
        //}

        public bool IsAlreadyUsedEvaluationId(string evaluationId)
        {
            TableQuery<Evaluation> rangeQuery = new TableQuery<Evaluation>().Where(
                TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("EvaluationId", QueryComparisons.Equal, evaluationId),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("InUse", QueryComparisons.Equal, "1")));

            List<Evaluation> existingEntities = evaluationTable.ExecuteQuery(rangeQuery).ToList();

            return existingEntities.Count > 0;
        }

        public void InsertEvaluationCodeTable(string evalId)
        {
            Evaluation evalCode = new Evaluation(evalId)
            {
                EvaluationId = evalId,
                InUse = "1"
            };
            TableOperation insertEvalCode = TableOperation.Insert(evalCode);
            evaluationTable.Execute(insertEvalCode);
        }

        #endregion
    }
}