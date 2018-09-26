using System;
using System.Collections.Generic;

namespace RDD.Domain.Models
{
    public abstract class EntityBase<TEntity, TKey> : IEntityBase<TEntity, TKey>
        where TEntity : class
        where TKey : IEquatable<TKey>
    {
        public abstract TKey Id { get; set; }
        public abstract string Name { get; set; }

        public virtual object GetId() => Id;

        public virtual void SetId(object id)
        {
            Id = (TKey)id;
        }

        public virtual TEntity Clone() => (TEntity)MemberwiseClone();
    }
}
