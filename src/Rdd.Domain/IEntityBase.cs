namespace Rdd.Domain
{
    public interface IEntityBase : IPrimaryKey
    {
        string Name { get; }
        string Url { get; }
    }

    public interface IEntityBase<out TKey> : IEntityBase, IPrimaryKey<TKey> { }
}
