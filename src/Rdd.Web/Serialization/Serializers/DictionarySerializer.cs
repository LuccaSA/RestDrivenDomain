using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using System;
using System.Collections;

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

        public void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields)
            => WriteJson(writer, entity as IDictionary, fields);

        protected void WriteJson(JsonTextWriter writer, IDictionary dico, IExpressionTree fields)
        {
            writer.WriteStartObject();

            if (fields.Children.Count != 0)
            {
                foreach (var child in fields.Children)
                {
                    var concreteChild = child.Node as ItemExpression;
                    WriteKvp(writer, NamingStrategy.GetDictionaryKey(concreteChild.Name), dico[concreteChild.Name], child);
                }
            }
            else
            {
                // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
                var enumerator = dico.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var entry = enumerator.Entry;
                    WriteKvp(writer, NamingStrategy.GetDictionaryKey(entry.Key.ToString()), entry.Value, fields);
                }
            }

            writer.WriteEndObject();
        }

        protected virtual void WriteKvp(JsonTextWriter writer, string key, object value, IExpressionTree fields)
        {
            writer.WritePropertyName(key, true);
            SerializerProvider.ResolveSerializer(value).WriteJson(writer, value, fields);
        }
    }
}