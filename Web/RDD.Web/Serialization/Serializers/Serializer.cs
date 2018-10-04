using Newtonsoft.Json;
using RDD.Domain.Helpers.Expressions;
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

        public abstract void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields);
    }
}