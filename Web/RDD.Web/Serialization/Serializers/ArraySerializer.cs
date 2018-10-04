using Newtonsoft.Json;
using NExtends.Primitives.Types;
using RDD.Domain.Helpers.Expressions;
using RDD.Web.Serialization.Providers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.Serialization.Serializers
{
    public class ArraySerializer : Serializer
    {
        public ArraySerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields)
        {
            var genericType = entity.GetType().GetEnumerableOrArrayElementType();
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