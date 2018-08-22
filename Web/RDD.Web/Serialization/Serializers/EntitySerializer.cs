using RDD.Domain;
using RDD.Web.Querying;
using RDD.Web.Serialization.Options;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using RDD.Web.Serialization.UrlProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RDD.Web.Serialization.Serializers
{
    public class EntitySerializer : ObjectSerializer
    {
        protected IUrlProvider UrlProvider { get; set; }

        public EntitySerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider, IUrlProvider urlProvider, Type workingType)
            : base(serializerProvider, reflectionProvider, workingType)
        {
            UrlProvider = urlProvider;
        }

        protected override SerializationOption RefineOptions(object entity, SerializationOption options)
        {
            if (options.Selectors == null || options.Selectors.Count == 0 || options.Selectors.Any(s => s?.Lambda == null))
            {
                options.Selectors = new FieldsParser().ParseFields(entity.GetType(), new List<string> { "id", "name" }).Select(p => p.EntitySelector).ToList();
            }

            return base.RefineOptions(entity, options);
        }

        protected override object GetRawValue(object entity, SerializationOption options, PropertyInfo property)
        {
            if (property.Name == "Url")
            {
                return UrlProvider?.GetEntityApiUri(WorkingType, entity as IPrimaryKey);
            }

            return base.GetRawValue(entity, options, property);
        }
    }
}