using System.Threading.Tasks;

namespace Rdd.Domain.Models
{
    public class DefaultInstanciator<TEntity> : IInstantiator<TEntity>
        where TEntity : class, new()
    {
        public Task<TEntity> InstantiateAsync(ICandidate<TEntity> candidate) => Task.FromResult(new TEntity());
    }
}