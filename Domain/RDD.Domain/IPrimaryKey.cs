namespace Rdd.Domain
{
    public interface IPrimaryKey
    {
        object GetId();
    }

    public interface IPrimaryKey<out TKey> : IPrimaryKey
    {
        TKey Id { get; }
    }
}