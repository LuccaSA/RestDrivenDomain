using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Json;
using RDD.Web.Serialization.Options;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using System.Collections;
using System.Linq;

namespace RDD.Web.Serialization.Serializers
{
    public class DictionarySerializer : ObjectSerializer
    {
        public DictionarySerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider) : base(serializerProvider, reflectionProvider, typeof(IDictionary)) { }

        public override IJsonElement ToJson(object entity, SerializationOption options)
        {
            return ToJson(entity as IDictionary, options);
        }

        protected IJsonElement ToJson(IDictionary dico, SerializationOption options)
        {
            if (options.Selectors.Any())
            {
                return ToJsonWithFieldsFilter(dico, options);
            }

            var result = new JsonObject();
            foreach (var key in dico.Keys)
            {
                //options may change according to serialized type
                SerializeKvp(result, key.ToString(), dico[key], new SerializationOption { Selectors = options.Selectors }, null);
            }

            return result;
        }

        private IJsonElement ToJsonWithFieldsFilter(IDictionary dico, SerializationOption options)
        {
            var result = new JsonObject();
            foreach (var child in options.Selectors.Select(c => c.Child))
            {
                var concreteChild = child as PropertySelector;
                try
                {
                    var value = concreteChild.Lambda.Compile().DynamicInvoke(dico);
                    SerializeKvp(result, concreteChild.Subject, value, new SerializationOption { Selectors = null }, null);
                }
                catch
                {
                    throw new BadRequestException($"Unknown property {concreteChild.Subject}");
                }
            }

            return result;
        }
    }
}