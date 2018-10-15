namespace Rdd.Domain.Models
{
    public interface IInstanciator<TEntity>
    {
        TEntity InstanciateNew(ICandidate<TEntity> candidate);
    }
}