using Microsoft.EntityFrameworkCore;

namespace RDD.Infra.Storage
{
    public interface IDbContextResolver
    {
        DbContext GetMatchingContext<TEntity>();
    }
}