using System.Threading.Tasks;

namespace Rdd.Domain
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
    }
}