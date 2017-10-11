namespace RDD.Domain
{
    public interface IPrimaryKey
    {
        object GetId();
        void SetId(object id);
    }

    public interface IPrimaryKey<TKey> : IPrimaryKey, IIdable<TKey> { }
}
