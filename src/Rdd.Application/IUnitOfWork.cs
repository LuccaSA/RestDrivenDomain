using System.Threading;
using System.Threading.Tasks;

namespace Rdd.Application
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}