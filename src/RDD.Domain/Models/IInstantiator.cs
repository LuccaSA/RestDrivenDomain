using System.Threading.Tasks;

namespace Rdd.Domain.Models
{
    public interface IInstantiator<TEntity>
    {
        Task<TEntity> InstantiateAsync(ICandidate<TEntity> candidate);
    }
}