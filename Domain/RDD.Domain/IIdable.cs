namespace RDD.Domain
{
    public interface IIdable<out TKey>
    {
        TKey Id { get; }
    }
}
