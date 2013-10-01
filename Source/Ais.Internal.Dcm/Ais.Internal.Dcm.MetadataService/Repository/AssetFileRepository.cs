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
    public class AssetFileRepository : RepositoryBase<AssetFileEntity>
    {
        public AssetFileRepository(CloudStorageAccount storageAccount)
            : base(storageAccount, "AssetFileTable")
        {

        }

        public override void Insert(AssetFileEntity item)
        {
            TableOperation insertOperation = TableOperation.Insert(item);
            table.Execute(insertOperation);
        }

        public override void Merge(AssetFileEntity item)
        {
            TableOperation mergeOperation = TableOperation.Merge(item);
            table.Execute(mergeOperation);
        }

        public override void Replace(AssetFileEntity item)
        {
            item.ETag = "*";
            TableOperation replaceOperation = TableOperation.Replace(item);
            table.Execute(replaceOperation);
        }

        public override void InsertOrMerge(AssetFileEntity item)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(item);
            table.Execute(insertOrMergeOperation);
        }

        public override void InsertOrReplace(AssetFileEntity item)
        {
            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(item);
            table.Execute(insertOrReplaceOperation);
        }

        public override void Delete(AssetFileEntity item)
        {
            TableOperation deleteOperation = TableOperation.Delete(item);
            table.Execute(deleteOperation);
        }

        public override Microsoft.WindowsAzure.Storage.Table.TableQuerySegment<AssetFileEntity> Find(string filterExpression, Microsoft.WindowsAzure.Storage.Table.TableContinuationToken continuationToken = null)
        {
            return ExecuteQuery(filterExpression, continuationToken);
        }
    }
}
