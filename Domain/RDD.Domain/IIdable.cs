namespace RDD.Domain
{
    public interface IIdable<TKey>
    {
        TKey Id { get; }
    }
}
