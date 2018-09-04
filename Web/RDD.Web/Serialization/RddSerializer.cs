using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Web.Models;
using RDD.Web.Serialization.Providers;
using System;
using System.Collections.Generic;

namespace RDD.Web.Serialization
{
    public class RDDSerializer : IRDDSerializer
    {
        private readonly ISerializerProvider _serializer;
        private readonly IPrincipal _principal;

        public RDDSerializer(ISerializerProvider serializer, IPrincipal principal)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _principal = principal;
        }

        public object Serialize<TEntity>(TEntity entity, Query<TEntity> query)
            where TEntity : class
            => new Metadata(_serializer.Serialize(entity, query.Fields), query.Options, query.Page, _principal).ToDictionary();

        public object Serialize<TEntity>(IEnumerable<TEntity> entities, Query<TEntity> query)
            where TEntity : class
            => new Metadata(_serializer.Serialize(entities, query.Fields), query.Options, query.Page, _principal).ToDictionary();

        public object Serialize<TEntity>(ISelection<TEntity> selection, Query<TEntity> query)
            where TEntity : class
            => new Metadata(_serializer.Serialize(selection, query.Fields), query.Options, query.Page, _principal).ToDictionary();
    }
}