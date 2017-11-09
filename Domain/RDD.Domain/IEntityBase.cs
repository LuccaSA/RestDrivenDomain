using RDD.Domain.Models;
using System.Collections.Generic;

namespace RDD.Domain
{
    public interface IEntityBase : IPrimaryKey, IIncludable
    {
        string Name { get; }
        string Url { get; }
    }

    public interface IEntityBase<TKey> : IEntityBase, IPrimaryKey<TKey> { }

    public interface IEntityBase<TEntity, TKey> : IEntityBase<TKey>, ICloneable<TEntity> { }
}
