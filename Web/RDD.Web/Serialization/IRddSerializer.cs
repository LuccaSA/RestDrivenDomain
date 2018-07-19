using RDD.Domain;
using RDD.Domain.Models.Querying;
using System.Collections.Generic;

namespace RDD.Web.Serialization
{
    public interface IRddSerializer
    {
        object Serialize<TEntity>(TEntity entity, Query<TEntity> query)
            where TEntity : class;
        object Serialize<TEntity>(IEnumerable<TEntity> entities, Query<TEntity> query)
            where TEntity : class;
        object Serialize<TEntity>(ISelection<TEntity> selection, Query<TEntity> query)
            where TEntity : class;
    }
}