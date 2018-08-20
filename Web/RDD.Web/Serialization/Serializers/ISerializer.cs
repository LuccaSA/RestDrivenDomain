using RDD.Domain.Json;
using RDD.Web.Serialization.Options;

namespace RDD.Web.Serialization.Serializers
{
	public interface ISerializer
	{
		IJsonElement ToJson(object entity, SerializationOption options);
	}
}
