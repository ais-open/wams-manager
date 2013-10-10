using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AzurePatterns.Entity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzurePatterns.Repository
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    public class AssetOutputRepository : RepositoryBase<AssetOutputEntity>
    {
        public AssetOutputRepository(CloudStorageAccount storageAccount)
            : base(storageAccount, "AssetOutputTable")
        {

        }

        public override void Insert(AssetOutputEntity item)
        {
            TableOperation insertOperation = TableOperation.Insert(item);
            table.Execute(insertOperation);
        }

        public override void Merge(AssetOutputEntity item)
        {
            TableOperation mergeOperation = TableOperation.Merge(item);
            table.Execute(mergeOperation);
        }

        public override void Replace(AssetOutputEntity item)
        {
            TableOperation replaceOperation = TableOperation.Replace(item);
            table.Execute(replaceOperation);
        }

        public override void InsertOrMerge(AssetOutputEntity item)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(item);
            table.Execute(insertOrMergeOperation);
        }

        public override void InsertOrReplace(AssetOutputEntity item)
        {
            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(item);
            table.Execute(insertOrReplaceOperation);
        }

        public override void Delete(AssetOutputEntity item)
        {
            TableOperation deleteOperation = TableOperation.Delete(item);
            table.Execute(deleteOperation);
        }

        public override Microsoft.WindowsAzure.Storage.Table.TableQuerySegment<AssetOutputEntity> Find(string filterExpression, Microsoft.WindowsAzure.Storage.Table.TableContinuationToken continuationToken = null)
        {
            return ExecuteQuery(filterExpression, continuationToken);
        }
    }
}
