﻿using Newtonsoft.Json;
using RDD.Domain.Helpers.Expressions;
using RDD.Web.Serialization.Providers;
using System;

namespace RDD.Web.Serialization.Serializers
{
    public class FuncSerializer<T> : Serializer
    {
        public FuncSerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields)
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