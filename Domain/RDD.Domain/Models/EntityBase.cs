using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdd.Domain.Models
{
    public abstract class EntityBase<TKey> : IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        public abstract TKey Id { get; set; }
        public abstract string Name { get; set; }

        [NotMapped]
        public virtual string Url { get; set; }

        public virtual object GetId() => Id;
    }
}