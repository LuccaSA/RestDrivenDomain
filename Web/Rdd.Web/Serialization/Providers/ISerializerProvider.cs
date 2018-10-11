using Newtonsoft.Json;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Serializers;
using System;

namespace Rdd.Web.Serialization.Providers
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