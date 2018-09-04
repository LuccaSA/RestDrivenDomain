using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Web.Serialization.Providers;

namespace RDD.Web.Serialization.Serializers
{
    public class ValueSerializer : Serializer
    {
        public ValueSerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override IJsonElement ToJson(object entity, IExpressionSelectorTree fields)
            => new JsonValue { Content = entity };
    }
}