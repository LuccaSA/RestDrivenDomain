using System;
using RDD.Domain.Json;
using RDD.Web.Serialization.Options;
using RDD.Web.Serialization.Providers;

namespace RDD.Web.Serialization.Serializers
{
	public class DateTimeSerializer : ValueSerializer
	{
		public DateTimeSerializer(ISerializerProvider serializerProvider) : base(serializerProvider) { }

		public override IJsonElement ToJson(object entity, SerializationOption options)
		{
			if (entity is DateTime)
				return ToJson((DateTime)entity, options);

			return base.ToJson(entity, options);
		}

		public IJsonElement ToJson(DateTime entity, SerializationOption options)
		{
			return new JsonValue { Content = DateTime.SpecifyKind(entity, DateTimeKind.Unspecified) };
		}
	}
}