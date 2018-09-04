using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using System.Collections.Generic;
using System.Reflection;

namespace RDD.Web.Serialization.Serializers
{
    public class CultureInfoSerializer : ObjectSerializer
    {
        private static HashSet<string> _allowedProperties = new HashSet<string> { "id", "name", "code" };

        public CultureInfoSerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider) : base(serializerProvider, reflectionProvider, typeof(Culture)) { }

        protected override void SerializeProperty(JsonObject partialResult, object entity, IExpressionSelectorTree fields, PropertyInfo property)
        {
            if (_allowedProperties.Contains(property.Name.ToLower()))
            {
                base.SerializeProperty(partialResult, entity, fields, property);
            }
        }
    }
}