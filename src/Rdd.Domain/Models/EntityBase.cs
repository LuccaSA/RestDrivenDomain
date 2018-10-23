using System;

namespace Rdd.Domain.Models
{
    public class EntityBase<TKey> : IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        public TKey Id { get; set; }
        public string Name { get; set; }

        public string Url { get; }

        object IPrimaryKey.GetId() => Id;
    }
}