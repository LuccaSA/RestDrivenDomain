using System;
using System.Threading.Tasks;

namespace Rdd.Application
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
    }
}