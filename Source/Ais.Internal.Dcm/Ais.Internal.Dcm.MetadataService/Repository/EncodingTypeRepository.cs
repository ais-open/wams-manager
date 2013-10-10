using AzurePatterns.Entity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzurePatterns.Repository
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    public class EncodingTypeRepository :RepositoryBase<EncodingTypeEntity>
    {
        public EncodingTypeRepository(CloudStorageAccount storageAccount)
            : base(storageAccount,"EncodingTypeTable")
        {

        }
        public override void Insert(EncodingTypeEntity item)
        {
            TableOperation insertOperation = TableOperation.Insert(item);
            table.Execute(insertOperation);
        }

        public override void Merge(EncodingTypeEntity item)
        {
            TableOperation mergeOperation = TableOperation.Merge(item);
            table.Execute(mergeOperation);
        }

        public override void Replace(EncodingTypeEntity item)
        {
            TableOperation replaceOperation = TableOperation.Replace(item);
            table.Execute(replaceOperation);
        }

        public override void InsertOrMerge(EncodingTypeEntity item)
        {
            TableOperation insertOrMerge = TableOperation.InsertOrMerge(item);
            table.Execute(insertOrMerge);
        }

        public override void InsertOrReplace(EncodingTypeEntity item)
        {
            TableOperation insertOrReplace = TableOperation.InsertOrReplace(item);
            table.Execute(insertOrReplace);
        }

        public override void Delete(EncodingTypeEntity item)
        {
            TableOperation deleteOperation = TableOperation.Delete(item);
            table.Execute(deleteOperation);
        }

        public override Microsoft.WindowsAzure.Storage.Table.TableQuerySegment<EncodingTypeEntity> Find(string filterExpression, Microsoft.WindowsAzure.Storage.Table.TableContinuationToken continuationToken = null)
        {
            return ExecuteQuery(filterExpression, continuationToken);
        }
    }
}
