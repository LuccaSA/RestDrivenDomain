using Newtonsoft.Json;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using System.Collections;

namespace Rdd.Web.Serialization.Serializers
{
    public class ArraySerializer : ISerializer
    {
        protected ISerializerProvider SerializerProvider { get; private set; }

        public ArraySerializer(ISerializerProvider serializerProvider)
        {
            SerializerProvider = serializerProvider;
        }

        public virtual void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields)
        {
            WriteJson(writer, (IEnumerable)entity, fields);
        }

        protected virtual void WriteJson(JsonTextWriter writer, IEnumerable entities, IExpressionTree fields)
        {
            writer.WriteStartArray();

            foreach (object entity in entities)
            {
                SerializerProvider.ResolveSerializer(entity).WriteJson(writer, entity, fields);
            }

            writer.WriteEndArray();
        }
    }
}