using Rdd.Domain;
using Rdd.Domain.Models.Querying;

namespace Rdd.Web.Querying
{
    public interface IRddObjectsMapper<TEntityDto, TEntity>
        where TEntityDto : class
        where TEntity : class
    {
        Query<TEntity> Map(Query<TEntityDto> query);

        ISelection<TEntityDto> Map(ISelection<TEntity> source);
        TEntityDto Map(TEntity source);
    }
}