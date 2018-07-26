using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System.Collections.Generic;

namespace RDD.Web.Serialization
{
    public interface IEntitySerializer
    {
        Dictionary<string, object> SerializeSelection<TEntity>(ISelection<TEntity> collection, Query<TEntity> query)
            where TEntity : class;
        List<Dictionary<string, object>> SerializeEntities<TEntity>(IEnumerable<TEntity> entities, IEnumerable<Field> fields);
        Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, IEnumerable<Field> fields);

        List<Dictionary<string, object>> SerializeEntities<TEntity>(IEnumerable<TEntity> entities, PropertySelector field);
        List<Dictionary<string, object>> SerializeEntities<TEntity>(IEnumerable<TEntity> entities, IEnumerable<PropertySelector> fields);
        Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, PropertySelector field);
        Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, IEnumerable<PropertySelector> fields);
    }
}
