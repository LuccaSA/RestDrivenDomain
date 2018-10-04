using Newtonsoft.Json;
using RDD.Domain.Helpers.Expressions;
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
        public static void WriteJson(this ISerializerProvider serializerProvider, JsonTextWriter writer, object entity, IExpressionTree fields)
            => serializerProvider.GetSerializer(entity).WriteJson(writer, entity, fields);
    }

}