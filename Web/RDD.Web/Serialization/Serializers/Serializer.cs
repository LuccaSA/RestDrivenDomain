using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Web.Serialization.Providers;

namespace RDD.Web.Serialization.Serializers
{
    public abstract class Serializer : ISerializer
    {
        protected ISerializerProvider SerializerProvider { get; private set; }

        protected Serializer(ISerializerProvider serializerProvider)
        {
            SerializerProvider = serializerProvider;
        }

        public abstract IJsonElement ToJson(object entity, IExpressionSelectorTree fields);
    }
}