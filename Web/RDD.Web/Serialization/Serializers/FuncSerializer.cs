using System;
using RDD.Domain.Json;
using RDD.Web.Serialization.Options;
using RDD.Web.Serialization.Providers;

namespace RDD.Web.Serialization.Serializers
{
    public class FuncSerializer<T> : Serializer
    {
        public FuncSerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override IJsonElement ToJson(object entity, SerializationOption options)
        {
            return ToJson(entity as Func<T>, options);
        }

        protected IJsonElement ToJson(Func<T> callback, SerializationOption options)
        {
            var serializer = SerializerProvider.GetSerializer(typeof(T));

            switch (serializer)
            {
                case ValueSerializer v: return v.ToJson(callback(), options);
                default:
                    return new JsonValue { Content = null };
            }
        }
    }
}