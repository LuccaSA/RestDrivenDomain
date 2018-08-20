using RDD.Domain;
using RDD.Domain.Json;
using RDD.Web.Serialization.Options;
using RDD.Web.Serialization.Providers;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.Serialization.Serializers
{
    public class SelectionSerializer : Serializer
	{
		public SelectionSerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

		public override IJsonElement ToJson(object entity, SerializationOption options)
		{
			return ToSerializableObject(entity as ISelection, options);
		}

		protected IJsonElement ToSerializableObject(ISelection collection, SerializationOption options)
		{
            return new JsonObject
            {
                Content = new Dictionary<string, IJsonElement>
                {
                   { "items", SerializerProvider.GetSerializer(collection.Items).ToJson(collection.Items, options) }
                }
            };
		}
	}
}