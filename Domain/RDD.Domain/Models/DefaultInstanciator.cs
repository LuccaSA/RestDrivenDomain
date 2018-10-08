namespace Rdd.Domain.Models
{
    public class DefaultInstanciator<TEntity> : IInstanciator<TEntity>
        where TEntity : class, new()
    {
        public TEntity InstanciateNew(ICandidate<TEntity> candidate) => new TEntity();
    }
}