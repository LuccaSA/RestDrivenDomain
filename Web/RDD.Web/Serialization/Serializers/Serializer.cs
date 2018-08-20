using RDD.Domain.Json;
using RDD.Web.Serialization.Options;
using RDD.Web.Serialization.Providers;

namespace RDD.Web.Serialization.Serializers
{
	public abstract class Serializer : ISerializer
	{
		public ISerializerProvider SerializerProvider { get; set; }

		public Serializer(ISerializerProvider serializerProvider)
		{
			SerializerProvider = serializerProvider;
		}

		public abstract IJsonElement ToJson(object entity, SerializationOption options);
	}
}
