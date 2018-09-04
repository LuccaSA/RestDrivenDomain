using RDD.Domain;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Web.Serialization.Providers;
using System.Collections.Generic;

namespace RDD.Web.Serialization.Serializers
{
    public class SelectionSerializer : Serializer
    {
        public SelectionSerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override IJsonElement ToJson(object entity, IExpressionSelectorTree fields)
        {
            return ToSerializableObject(entity as ISelection, fields);
        }

        protected IJsonElement ToSerializableObject(ISelection collection, IExpressionSelectorTree fields)
        {
            var items = collection.GetItems();
            return new JsonObject
            {
                Content = new Dictionary<string, IJsonElement>
                {
                   { "items", SerializerProvider.GetSerializer(items).ToJson(items, fields) }
                }
            };
        }
    }
}