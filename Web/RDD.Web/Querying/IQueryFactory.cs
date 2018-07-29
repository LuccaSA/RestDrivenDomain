using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Querying
{
    public interface IQueryFactory
    {
        Query<TEntity> NewFromHttpRequest<TEntity, TKey>(HttpVerbs? verb)
            where TEntity : class, IPrimaryKey<TKey>;

        Query<TEntity> New<TEntity>()
            where TEntity : class;
    }
}