using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Web.Models;
using RDD.Web.Serialization.Options;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.UrlProviders;
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
            => new Metadata(_serializer.Serialize(entity, new SerializationOption(query.Fields)), query.Options, query.Page, _principal).ToDictionary();

        public object Serialize<TEntity>(IEnumerable<TEntity> entities, Query<TEntity> query)
            where TEntity : class
            => new Metadata(_serializer.Serialize(entities, new SerializationOption(query.Fields)), query.Options, query.Page, _principal).ToDictionary();

        public object Serialize<TEntity>(ISelection<TEntity> selection, Query<TEntity> query)
            where TEntity : class
            => new Metadata(_serializer.Serialize(selection, new SerializationOption(query.Fields)), query.Options, query.Page, _principal).ToDictionary();
    }
}