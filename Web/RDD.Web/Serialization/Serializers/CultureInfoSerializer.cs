using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Web.Serialization.Providers;
using System.Collections.Generic;
using System.Reflection;

namespace Rdd.Web.Serialization.Serializers
{
    public class CultureInfoSerializer : ObjectSerializer
    {
        private static HashSet<string> _allowedProperties = new HashSet<string> { "id", "name", "code" };

        public CultureInfoSerializer(ISerializerProvider serializerProvider, IReflectionHelper reflectionHelper, NamingStrategy namingStrategy)
            : base(serializerProvider, reflectionHelper, namingStrategy) { }

        protected override void SerializeProperty(JsonTextWriter writer, object entity, IExpressionTree fields, PropertyInfo property)
        {
            if (_allowedProperties.Contains(property.Name.ToLower()))
            {
                base.SerializeProperty(writer, entity, fields, property);
            }
        }
    }
}