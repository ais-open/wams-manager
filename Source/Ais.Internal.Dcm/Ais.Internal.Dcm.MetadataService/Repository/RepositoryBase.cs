using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzurePatterns.Repository
{
    public abstract class RepositoryBase<T> where T : TableEntity, new()
    {
        protected readonly string tableName;

        protected readonly CloudTable table;

        public RepositoryBase(CloudStorageAccount storageAccount, string tableName)
        {
            this.tableName = tableName;
            var tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference(tableName);
            table.CreateIfNotExists();
        }

        public abstract void Insert(T item);

        public abstract void Merge(T item);

        public abstract void Replace(T item);

        public abstract void InsertOrMerge(T item);

        public abstract void InsertOrReplace(T item);

        public abstract void Delete(T item);

        public abstract TableQuerySegment<T> Find(string filterExpression, TableContinuationToken continuationToken = null);

        protected internal TableQuerySegment<T> ExecuteQuery(string filterExpression, TableContinuationToken continuationToken = null)
        {
            TableQuery<T> query = new TableQuery<T>().Where(filterExpression);
            return table.ExecuteQuerySegmented<T>(query, continuationToken);
        }
    }
}
