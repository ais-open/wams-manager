using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AzurePatterns.Entity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzurePatterns.Repository
{
    public class TagRepository : RepositoryBase<TagEntity>
    {
        public TagRepository(CloudStorageAccount storageAccount) : base(storageAccount,"TagTable")
        {
            
        }
        
        public override void Insert(TagEntity item)
        {
            TableOperation insertOperation = TableOperation.Insert(item);
            table.Execute(insertOperation);
        }

        public override void Merge(TagEntity item)
        {
            TableOperation mergeOperation = TableOperation.Merge(item);
            table.Execute(mergeOperation);
        }

        public override void Replace(TagEntity item)
        {
            TableOperation replaceOperation = TableOperation.Replace(item);
            table.Execute(replaceOperation);
        }

        public override void InsertOrMerge(TagEntity item)
        {
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(item);
            table.Execute(insertOrMergeOperation);
        }

        public override void InsertOrReplace(TagEntity item)
        {
            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(item);
            table.Execute(insertOrReplaceOperation);
        }

        public override void Delete(TagEntity item)
        {
            TableOperation deleteOperation = TableOperation.Delete(item);
            table.Execute(deleteOperation);
        }

        public override TableQuerySegment<TagEntity> Find(string filterExpression, Microsoft.WindowsAzure.Storage.Table.TableContinuationToken continuationToken = null)
        {
            return ExecuteQuery(filterExpression, continuationToken);
        }
    }
}
