namespace Rdd.Domain
{
    public interface IPrimaryKey
    {
        object GetId();
        void SetId(object id);
    }

    public interface IPrimaryKey<out TKey> : IPrimaryKey
    {
        TKey Id { get; }
    }
}