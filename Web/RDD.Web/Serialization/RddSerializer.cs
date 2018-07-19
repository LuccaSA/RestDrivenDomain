using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Web.Models;
using System;
using System.Collections.Generic;

namespace RDD.Web.Serialization
{
    public class RddSerializer : IRddSerializer
    {
        private readonly IEntitySerializer _serializer;
        private readonly IPrincipal _principal;

        public RddSerializer(IEntitySerializer serializer, IPrincipal principal)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _principal = principal ?? throw new ArgumentNullException(nameof(principal));
        }

        public object Serialize<TEntity>(TEntity entity, Query<TEntity> query)
            where TEntity : class
            => new Metadata(_serializer.SerializeEntity(entity, query.Fields), query.Options, query.Page, _principal).ToDictionary();

        public object Serialize<TEntity>(IEnumerable<TEntity> entities, Query<TEntity> query)
            where TEntity : class
            => new Metadata(_serializer.SerializeEntities(entities, query.Fields), query.Options, query.Page, _principal).ToDictionary();

        public object Serialize<TEntity>(ISelection<TEntity> selection, Query<TEntity> query)
            where TEntity : class
            => new Metadata(_serializer.SerializeSelection(selection, query), query.Options, query.Page, _principal).ToDictionary();
    }
}