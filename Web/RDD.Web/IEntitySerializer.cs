using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RDD.Web
{
    public interface IEntitySerializer
    {
        Dictionary<string, object> SerializeSelection(ISelection collection, Query query);
        List<Dictionary<string, object>> SerializeEntities(IEnumerable entities, Field fields);
        Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, Field fields);

        List<Dictionary<string, object>> SerializeEntities(IEnumerable entities, PropertySelector fields);
        Dictionary<string, object> SerializeEntity<TEntity>(TEntity entity, PropertySelector fields);
    }
}
