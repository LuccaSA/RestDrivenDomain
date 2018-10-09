namespace Rdd.Domain
{
    public interface IEntityBase : IPrimaryKey
    {
        string Name { get; }
        string Url { get; }
    }

    public interface IEntityBase<TKey> : IEntityBase, IPrimaryKey<TKey> { }

    public interface IEntityBase<TEntity, TKey> : IEntityBase<TKey>{ }
}
