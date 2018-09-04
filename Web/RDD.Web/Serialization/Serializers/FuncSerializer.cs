using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Web.Serialization.Providers;
using System;

namespace RDD.Web.Serialization.Serializers
{
    public class FuncSerializer<T> : Serializer
    {
        public FuncSerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override IJsonElement ToJson(object entity, IExpressionSelectorTree fields)
        {
            return ToJson(entity as Func<T>, fields);
        }

        protected IJsonElement ToJson(Func<T> callback, IExpressionSelectorTree fields)
        {
            var serializer = SerializerProvider.GetSerializer(typeof(T));

            switch (serializer)
            {
                case ValueSerializer v: return v.ToJson(callback(), fields);
                default:
                    return new JsonValue { Content = null };
            }
        }
    }
}