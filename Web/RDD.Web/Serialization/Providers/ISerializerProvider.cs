using System;
using RDD.Domain.Json;
using RDD.Web.Serialization.Options;
using RDD.Web.Serialization.Serializers;

namespace RDD.Web.Serialization.Providers
{
    public interface ISerializerProvider
    {
        ISerializer GetSerializer(object entity);
        ISerializer GetSerializer(Type type);
    }

    public static class ISerializerProviderExtensions
    {
        public static object Serialize(this ISerializerProvider serializerProvider, object entity, SerializationOption options)
            => serializerProvider.ToJson(entity, options).GetContent();

        public static IJsonElement ToJson(this ISerializerProvider serializerProvider, object entity, SerializationOption options)
            => serializerProvider.GetSerializer(entity).ToJson(entity, options);
    }

}