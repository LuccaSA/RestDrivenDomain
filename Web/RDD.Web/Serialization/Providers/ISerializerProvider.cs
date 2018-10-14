﻿using Newtonsoft.Json;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Serializers;

namespace Rdd.Web.Serialization.Providers
{
    public interface ISerializerProvider
    {
        ISerializer ResolveSerializer(object entity);
    }

    public static class ISerializerProviderExtensions
    {
        public static void WriteJson(this ISerializerProvider serializerProvider, JsonTextWriter writer, object entity, IExpressionTree fields)
            => serializerProvider.ResolveSerializer(entity).WriteJson(writer, entity, fields);
    }

}