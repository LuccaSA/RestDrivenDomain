using Newtonsoft.Json;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
            WriteJson(writer, (entity as IEnumerable).Cast<object>(), fields);
        }

        protected virtual void WriteJson(JsonTextWriter writer, IEnumerable<object> entities, IExpressionTree fields)
        {
            writer.WriteStartArray();

            foreach (var entity in entities)
            {
                SerializerProvider.GetSerializer(entity).WriteJson(writer, entity, fields);
            }

            writer.WriteEndArray();
        }
    }
}