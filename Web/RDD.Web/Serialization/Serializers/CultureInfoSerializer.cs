using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using System.Collections.Generic;
using System.Reflection;

namespace RDD.Web.Serialization.Serializers
{
    public class CultureInfoSerializer : ObjectSerializer
    {
        private static HashSet<string> _allowedProperties = new HashSet<string> { "id", "name", "code" };

        public CultureInfoSerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider, NamingStrategy namingStrategy)
            : base(serializerProvider, reflectionProvider, namingStrategy, typeof(Culture)) { }

        protected override void SerializeProperty(JsonTextWriter writer, object entity, IExpressionTree fields, PropertyInfo property)
        {
            if (_allowedProperties.Contains(property.Name.ToLower()))
            {
                base.SerializeProperty(writer, entity, fields, property);
            }
        }
    }
}