using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Web.Serialization.Providers;

namespace RDD.Web.Serialization.Serializers
{
    public class ToStringSerializer : ValueSerializer
    {
        public ToStringSerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override IJsonElement ToJson(object entity, IExpressionSelectorTree fields)
            => base.ToJson(entity.ToString(), fields);
    }
}