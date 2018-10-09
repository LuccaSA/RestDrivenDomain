using System;
using System.Threading.Tasks;

namespace Rdd.Application
{
    public interface IUnitOfWork : IDisposable
    {
        Task SaveChangesAsync();
    }
}