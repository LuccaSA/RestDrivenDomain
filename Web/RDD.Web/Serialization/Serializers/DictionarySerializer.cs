using RDD.Domain.Exceptions;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using System.Collections;
using System.Linq;

namespace RDD.Web.Serialization.Serializers
{
    public class DictionarySerializer : ObjectSerializer
    {
        public DictionarySerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider) : base(serializerProvider, reflectionProvider, typeof(IDictionary)) { }

        public override IJsonElement ToJson(object entity, IExpressionSelectorTree fields)
        {
            return ToJson(entity as IDictionary, fields);
        }

        protected IJsonElement ToJson(IDictionary dico, IExpressionSelectorTree fields)
        {
            if (fields.Children.Any())
            {
                return ToJsonWithFieldsFilter(dico, fields);
            }

            var result = new JsonObject();
            foreach (var key in dico.Keys)
            {
                SerializeKvp(result, key.ToString(), dico[key], fields, null);
            }

            return result;
        }

        private IJsonElement ToJsonWithFieldsFilter(IDictionary dico, IExpressionSelectorTree fields)
        {
            var result = new JsonObject();
            foreach (var child in fields.Children)
            {
                var concreteChild = child.Node as ItemSelector;
                try
                {
                    var value = dico[concreteChild.Name];
                    SerializeKvp(result, concreteChild.Name, value, child, null);
                }
                catch
                {
                    throw new BadRequestException($"Unknown property {concreteChild.Name }");
                }
            }

            return result;
        }
    }
}