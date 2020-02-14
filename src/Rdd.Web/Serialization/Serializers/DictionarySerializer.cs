using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace Rdd.Web.Serialization.Serializers
{
    public class DictionarySerializer : ISerializer
    {
        protected ISerializerProvider SerializerProvider { get; private set; }
        protected NamingStrategy NamingStrategy { get; private set; }
        
        public DictionarySerializer(ISerializerProvider serializerProvider, NamingStrategy namingStrategy)
        {
            SerializerProvider = serializerProvider ?? throw new ArgumentNullException(nameof(serializerProvider));
            NamingStrategy = namingStrategy ?? throw new ArgumentNullException(nameof(namingStrategy));
        }

        public Task WriteJsonAsync(JsonTextWriter writer, object entity, IExpressionTree fields)
            => WriteJsonAsync(writer, entity as IDictionary, fields);

        protected async Task WriteJsonAsync(JsonTextWriter writer, IDictionary dico, IExpressionTree fields)
        {
            await writer.WriteStartObjectAsync();

            if (fields.Children.Count != 0)
            {
                foreach (var child in fields.Children)
                {
                    var concreteChild = child.Node as ItemExpression;
                    await WriteKvpAsync(writer, NamingStrategy.GetDictionaryKey(concreteChild.Name), dico[concreteChild.Name], child);
                }
            }
            else
            {
                // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
                var enumerator = dico.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var entry = enumerator.Entry;
                    await WriteKvpAsync(writer, NamingStrategy.GetDictionaryKey(entry.Key.ToString()), entry.Value, fields);
                }
            }

            await writer.WriteEndObjectAsync();
        }

        protected virtual async Task WriteKvpAsync(JsonTextWriter writer, string key, object value, IExpressionTree fields)
        {
            await writer.WritePropertyNameAsync(key, true);
            await SerializerProvider.ResolveSerializer(value).WriteJsonAsync(writer, value, fields);
        }
    }
}