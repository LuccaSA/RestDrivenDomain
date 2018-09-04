using NExtends.Primitives.Types;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Web.Serialization.Providers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.Serialization.Serializers
{
    public class ArraySerializer : Serializer
    {
        public ArraySerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override IJsonElement ToJson(object entity, IExpressionSelectorTree fields)
        {
            var genericType = entity.GetType().GetEnumerableOrArrayElementType();
            return ToJson(genericType, (entity as IEnumerable).Cast<object>(), fields);
        }

        protected virtual IJsonElement ToJson(Type genericType, IEnumerable<object> entities, IExpressionSelectorTree fields)
        {
            if (genericType == typeof(object))
            {
                return new JsonArray
                {
                    Content = entities.Select(e => SerializerProvider.GetSerializer(e.GetType()).ToJson(e, fields)).ToList()
                };
            }
            else
            {
                var serializer = SerializerProvider.GetSerializer(genericType);
                return new JsonArray
                {
                    Content = entities.Select(e => e == null ? null : serializer.ToJson(e, fields)).ToList()
                };
            }
        }
    }
}