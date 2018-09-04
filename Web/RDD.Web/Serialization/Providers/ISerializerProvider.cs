using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Web.Serialization.Serializers;
using System;

namespace RDD.Web.Serialization.Providers
{
    public interface ISerializerProvider
    {
        ISerializer GetSerializer(object entity);
        ISerializer GetSerializer(Type type);
    }

    public static class ISerializerProviderExtensions
    {
        public static object Serialize(this ISerializerProvider serializerProvider, object entity, IExpressionSelectorTree fields)
            => serializerProvider.ToJson(entity, fields).GetContent();

        public static IJsonElement ToJson(this ISerializerProvider serializerProvider, object entity, IExpressionSelectorTree fields)
            => serializerProvider.GetSerializer(entity).ToJson(entity, fields);
    }

}