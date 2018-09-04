using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Web.Serialization.Providers;
using System;

namespace RDD.Web.Serialization.Serializers
{
    public class DateTimeSerializer : ValueSerializer
    {
        public DateTimeSerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override IJsonElement ToJson(object entity, IExpressionSelectorTree fields)
        {
            switch (entity)
            {
                case DateTime d: return ToJson(d, fields);
                default: return base.ToJson(entity, fields);
            }
        }

        public IJsonElement ToJson(DateTime entity, IExpressionSelectorTree fields)
        {
            return new JsonValue { Content = DateTime.SpecifyKind(entity, DateTimeKind.Unspecified) };
        }
    }
}