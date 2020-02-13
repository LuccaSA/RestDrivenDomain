using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rdd.Web.Serialization.Serializers
{
    public class CultureInfoSerializer : ObjectSerializer
    {
        private static readonly HashSet<string> _allowedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "id", "name", "code" };

        public CultureInfoSerializer(ISerializerProvider serializerProvider, NamingStrategy namingStrategy)
            : base(serializerProvider, namingStrategy) { }

        protected override Task SerializePropertyAsync(JsonTextWriter writer, object entity, IExpressionTree fields, PropertyExpression property)
        {
            if (_allowedProperties.Contains(property.Name))
            {
                return base.SerializePropertyAsync(writer, entity, fields, property);
            }
            return Task.CompletedTask;
        }
    }
}