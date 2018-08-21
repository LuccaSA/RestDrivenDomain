using RDD.Domain.Json;
using RDD.Web.Serialization.Options;
using RDD.Web.Serialization.Providers;

namespace RDD.Web.Serialization.Serializers
{
    public class ToStringSerializer : ValueSerializer
    {
        public ToStringSerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override IJsonElement ToJson(object entity, SerializationOption options)
            => base.ToJson(entity.ToString(), options);
    }
}