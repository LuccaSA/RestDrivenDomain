using System;
using RDD.Domain.Json;
using RDD.Web.Serialization.Options;
using RDD.Web.Serialization.Serializers;

namespace RDD.Web.Serialization.Providers
{
	public interface ISerializerProvider
	{
		/// <summary>
		/// Raccourci pour GetSerializer(entity).ToJson(entity, options).Serialize();
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		object Serialize(object entity, SerializationOption options);
        IJsonElement ToJson(object entity, SerializationOption options);

        ISerializer GetSerializer(object entity);
		ISerializer GetSerializer(Type type);
	}
}