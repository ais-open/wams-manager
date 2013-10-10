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
    public class AssetThumbnailRepository : RepositoryBase<AssetThumbnailEntity>
    {
        public AssetThumbnailRepository(CloudStorageAccount storageAccount)
            : base(storageAccount, "AssetThumbnailTable")
        {

        }

        public override void Insert(AssetThumbnailEntity item)
        {
            TableOperation insertOperation = TableOperation.Insert(item);
            table.Execute(insertOperation);
        }

        public override void Merge(AssetThumbnailEntity item)
        {
            TableOperation mergeOperation = TableOperation.Merge(item);
            table.Execute(mergeOperation);
        }

        public override void Replace(AssetThumbnailEntity item)
        {
            TableOperation replaceOperation = TableOperation.Replace(item);
            table.Execute(replaceOperation);
        }

        public override void InsertOrMerge(AssetThumbnailEntity item)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(item);
            table.Execute(insertOrMergeOperation);
        }

        public override void InsertOrReplace(AssetThumbnailEntity item)
        {
            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(item);
            table.Execute(insertOrReplaceOperation);
        }

        public override void Delete(AssetThumbnailEntity item)
        {
            TableOperation deleteOperation = TableOperation.Delete(item);
            table.Execute(deleteOperation);
        }

        public override Microsoft.WindowsAzure.Storage.Table.TableQuerySegment<AssetThumbnailEntity> Find(string filterExpression, Microsoft.WindowsAzure.Storage.Table.TableContinuationToken continuationToken = null)
        {
            return ExecuteQuery(filterExpression, continuationToken);
        }
    }
}
