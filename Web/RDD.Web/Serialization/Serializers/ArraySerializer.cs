using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NExtends.Primitives.Types;
using RDD.Domain.Json;
using RDD.Web.Serialization.Options;
using RDD.Web.Serialization.Providers;

namespace RDD.Web.Serialization.Serializers
{
    public class ArraySerializer : Serializer
    {
        public ArraySerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override IJsonElement ToJson(object entity, SerializationOption options)
        {
            var genericType = entity.GetType().GetEnumerableOrArrayElementType();
            return ToJson(genericType, (entity as IEnumerable).Cast<object>(), options);
        }

        protected virtual IJsonElement ToJson(Type genericType, IEnumerable<object> entities, SerializationOption options)
        {
            if (genericType == typeof(object))
            {
                return new JsonArray
                {
                    Content = entities.Select(e => SerializerProvider.GetSerializer(e.GetType()).ToJson(e, options)).ToList()
                };
            }
            else
            {
                var serializer = SerializerProvider.GetSerializer(genericType);
                return new JsonArray
                {
                    Content = entities.Select(e => e == null ? null : serializer.ToJson(e, options)).ToList()
                };
            }
        }
    }
}