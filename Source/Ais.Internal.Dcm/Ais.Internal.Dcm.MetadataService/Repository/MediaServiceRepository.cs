using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AzurePatterns.Entity;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;

namespace AzurePatterns.Repository
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    public class MediaServiceRepository : RepositoryBase<MediaServiceEntity>
    {
        public MediaServiceRepository(CloudStorageAccount storageAccount, string clientID)
            : base(storageAccount,string.Format("{0}MediaServiceTable",clientID))
        {

        }

        public override void Insert(MediaServiceEntity item)
        {
            TableOperation insertOperation = TableOperation.Insert(item);
            table.Execute(insertOperation);
        }

        public override void Merge(MediaServiceEntity item)
        {
            TableOperation mergeOperation = TableOperation.Merge(item);
            table.Execute(mergeOperation);
        }

        public override void Replace(MediaServiceEntity item)
        {
            TableOperation replaceOperation = TableOperation.Replace(item);
            table.Execute(replaceOperation);
        }

        public override void InsertOrMerge(MediaServiceEntity item)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(item);
            table.Execute(insertOrMergeOperation);
        }

        public override void InsertOrReplace(MediaServiceEntity item)
        {
            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(item);
            table.Execute(insertOrReplaceOperation);
        }

        public override void Delete(MediaServiceEntity item)
        {
            item.ETag = "*";
            TableOperation deleteOperation = TableOperation.Delete(item);
            table.Execute(deleteOperation);
        }

        public override TableQuerySegment<MediaServiceEntity> Find(string filterExpression, TableContinuationToken continuationToken = null)
        {
            return ExecuteQuery(filterExpression, continuationToken);
        }
    }
}
