using RDD.Domain.Helpers;
using RDD.Domain.Json;
using RDD.Web.Serialization.Options;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using System;
using System.Linq;
using System.Reflection;

namespace RDD.Web.Serialization.Serializers
{
    public class ObjectSerializer : Serializer
	{
		protected IReflectionProvider _reflectionProvider;

		public Type WorkingType { get; set; }

		public ObjectSerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider, Type workingType)
			: base(serializerProvider)
		{
			_reflectionProvider = reflectionProvider;
			WorkingType = workingType;
		}

        public override IJsonElement ToJson(object entity, SerializationOption options)
        {
            var result = new JsonObject();
            var correctedOptions = RefineOptions(entity, options);

            foreach (var subSelectors in correctedOptions.Selectors.GroupBy(s => s.Name))
            {
                SerializeProperty(result, entity, new SerializationOption { Selectors = subSelectors.ToList() });
            }

            return result;
        }

		protected virtual SerializationOption RefineOptions(object entity, SerializationOption options)
		{
            if (options == null || options.Selectors == null || options.Selectors.Any(s => s?.Lambda == null))
            {
                options.Selectors = _reflectionProvider.GetProperties(WorkingType).Select(p =>
                {
                    var selector = PropertySelector.NewFromType(WorkingType, null);
                    selector.Parse(p.Name);
                    return selector;
                }).ToList();
            }

			return options;
		}

        protected virtual void SerializeProperty(JsonObject partialResult, object entity, SerializationOption options)
        {
            var property = options.Selectors.First(s => s != null).GetCurrentProperty();
            SerializeProperty(partialResult, entity, options, property);
        }

		protected virtual void SerializeProperty(JsonObject partialResult, object entity, SerializationOption options, PropertyInfo property)
		{
			var key = GetKey(entity, options, property);
			var value = GetRawValue(entity, options, property);

			SerializeKvp(partialResult, key, value, options, property);
		}

		protected virtual void SerializeKvp(JsonObject partialResult, string key, object value, SerializationOption options, PropertyInfo property)
		{
            var subOptions = new SerializationOption { Selectors = options.Selectors.Select(s => s.Child).ToList() };

            partialResult.Content[key] = SerializerProvider.GetSerializer(value).ToJson(value, subOptions);
		}

		protected virtual string GetKey(object entity, SerializationOption options, PropertyInfo property)
		{
			return property.Name;
		}

		protected virtual object GetRawValue(object entity, SerializationOption options, PropertyInfo property)
		{
			return property.GetValue(entity, null);
		}
	}
}