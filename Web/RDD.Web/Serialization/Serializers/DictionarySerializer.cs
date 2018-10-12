using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Web.Serialization.Providers;
using System.Collections;
using System.Linq;

namespace Rdd.Web.Serialization.Serializers
{
    public class DictionarySerializer : ObjectSerializer
    {
        public DictionarySerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider, NamingStrategy namingStrategy) 
            : base(serializerProvider, reflectionProvider, namingStrategy) { }

        public override void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields)
            => WriteJson(writer, entity as IDictionary, fields);

        protected void WriteJson(JsonTextWriter writer, IDictionary dico, IExpressionTree fields)
        {
            writer.WriteStartObject();

            if (fields.Children.Any())
            {
                foreach (var child in fields.Children)
                {
                    var concreteChild = child.Node as ItemExpression;
                    try
                    {
                        WriteKvp(writer, NamingStrategy.GetDictionaryKey(concreteChild.Name), dico[concreteChild.Name], child, null);
                    }
                    catch
                    {
                        throw new BadRequestException($"Unknown key {concreteChild.Name }");
                    }
                }
            }
            else
            {
                foreach (var key in dico.Keys)
                {
                    WriteKvp(writer, NamingStrategy.GetDictionaryKey(key.ToString()), dico[key], fields, null);
                }
            }

            writer.WriteEndObject();
        }
    }
}