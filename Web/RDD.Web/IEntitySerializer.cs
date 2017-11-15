using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;

namespace RDD.Web
{
    public interface IEntitySerializer
    {
        Dictionary<string, object> SerializeSelection<TEntity>(ISelection<TEntity> collection, Query<TEntity> query)
            where TEntity : class, IEntityBase;
        List<Dictionary<string, object>> SerializeEntities<TEntity>(IEnumerable<TEntity> entities, Field<TEntity> fields);
        Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, Field<TEntity> fields);

        List<Dictionary<string, object>> SerializeEntities<TEntity>(IEnumerable<TEntity> entities, PropertySelector fields);
        Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, PropertySelector fields);
    }
}
