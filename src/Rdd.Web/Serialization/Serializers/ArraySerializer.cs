using Newtonsoft.Json;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using System.Collections;
using System.Threading.Tasks;

namespace Rdd.Web.Serialization.Serializers
{
    public class ArraySerializer : ISerializer
    {
        protected ISerializerProvider SerializerProvider { get; private set; }

        public ArraySerializer(ISerializerProvider serializerProvider)
        {
            SerializerProvider = serializerProvider;
        }

        public virtual Task WriteJsonAsync(JsonTextWriter writer, object entity, IExpressionTree fields)
        {
            return WriteJsonAsync(writer, (IEnumerable)entity, fields);
        }

        protected async virtual Task WriteJsonAsync(JsonTextWriter writer, IEnumerable entities, IExpressionTree fields)
        {
            await writer.WriteStartArrayAsync();

            foreach (object entity in entities)
            {
                await SerializerProvider.ResolveSerializer(entity).WriteJsonAsync(writer, entity, fields);
            }

            await writer.WriteEndArrayAsync();
        }
    }
}