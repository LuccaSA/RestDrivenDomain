using Newtonsoft.Json;
using RDD.Domain.Helpers.Expressions;
using RDD.Web.Serialization.Providers;

namespace RDD.Web.Serialization.Serializers
{
    public class ToStringSerializer : ValueSerializer
    {
        public ToStringSerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields)
            => writer.WriteValue(entity.ToString());
    }
}