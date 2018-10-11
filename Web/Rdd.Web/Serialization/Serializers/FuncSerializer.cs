using Newtonsoft.Json;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using System;

namespace Rdd.Web.Serialization.Serializers
{
    public class FuncSerializer<T>
    {
        protected ISerializerProvider SerializerProvider { get; private set; }

        public FuncSerializer(ISerializerProvider serializerProvider)
        {
            SerializerProvider = serializerProvider;
        }

        public virtual void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields)
            => WriteJson(writer, entity as Func<T>, fields);

        protected void WriteJson(JsonTextWriter writer, Func<T> callback, IExpressionTree fields)
        {
            var serializer = SerializerProvider.GetSerializer(typeof(T));

            switch (serializer)
            {
                case ValueSerializer v:
                    v.WriteJson(writer, callback(), fields);
                    break;

                default:
                    writer.WriteNull();
                    break;
            }
        }
    }
}