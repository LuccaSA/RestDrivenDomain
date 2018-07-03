namespace RDD.Domain
{
    public interface IEntityBase : IPrimaryKey, IIncludable
    {
        string Name { get; }
    }

    public interface IEntityBase<TKey> : IEntityBase, IPrimaryKey<TKey> { }

    public interface IEntityBase<TEntity, TKey> : IEntityBase<TKey>, ICloneable<TEntity> { }
}
