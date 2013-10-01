namespace AzurePatterns
{
    using System;
    using System.Linq.Expressions;

    public abstract class Specification<TEntity>
    {
        public abstract Expression<Func<TEntity, bool>> Predicate { get; }
    }

    public class Filter
    {
        public string KeyValue { get; set; }


        public KeyType KeyType { get; set; }
    }

    public enum KeyType
    {
        PartitionKey,
        RowKey
    }

}
