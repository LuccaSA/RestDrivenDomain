using Newtonsoft.Json;
using RDD.Domain.Helpers.Expressions;
using RDD.Web.Serialization.Providers;
using System;

namespace RDD.Web.Serialization.Serializers
{
    public class DateTimeSerializer : ValueSerializer
    {
        public DateTimeSerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

        public override void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields)
        {
            switch (entity)
            {
                case DateTime d:
                    writer.WriteValue(DateTime.SpecifyKind(d, DateTimeKind.Unspecified));
                    break;

                default:
                    base.WriteJson(writer, entity, fields);
                    break;
            }
        }
    }
}