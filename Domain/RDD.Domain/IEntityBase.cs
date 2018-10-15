namespace Rdd.Domain
{
    public interface IEntityBase : IPrimaryKey
    {
        string Name { get; }
        string Url { get; }
    }

    public interface IEntityBase<TKey> : IEntityBase, IPrimaryKey<TKey> { }
}
