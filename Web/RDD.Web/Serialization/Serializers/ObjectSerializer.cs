using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using System;
using System.Linq;
using System.Linq.Expressions;
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

        public override IJsonElement ToJson(object entity, IExpressionSelectorTree fields)
        {
            var result = new JsonObject();
            var correctedFields = CorrectFields(entity, fields);

            foreach (var subSelector in correctedFields.Children)
            {
                SerializeProperty(result, entity, subSelector);
            }

            return result;
        }

        protected virtual IExpressionSelectorTree CorrectFields(object entity, IExpressionSelectorTree fields)
        {
            if (fields == null || !fields.Children.Any())
            {
                return new ExpressionSelectorParser().ParseTree(WorkingType,
                    string.Join(",", _reflectionProvider.GetProperties(WorkingType).Select(p => p.Name)));
            }

            return fields;
        }

        protected virtual void SerializeProperty(JsonObject partialResult, object entity, IExpressionSelectorTree fields)
        {
            var property = (fields.Node.ToLambdaExpression().Body as MemberExpression).Member as PropertyInfo;
            SerializeProperty(partialResult, entity, fields, property);
        }

        protected virtual void SerializeProperty(JsonObject partialResult, object entity, IExpressionSelectorTree fields, PropertyInfo property)
        {
            var key = GetKey(entity, fields, property);
            var value = GetRawValue(entity, fields, property);

            SerializeKvp(partialResult, key, value, fields, property);
        }

        protected virtual void SerializeKvp(JsonObject partialResult, string key, object value, IExpressionSelectorTree fields, PropertyInfo property)
        {
            partialResult.Content[key] = SerializerProvider.GetSerializer(value).ToJson(value, fields);
        }

        protected virtual string GetKey(object entity, IExpressionSelectorTree fields, PropertyInfo property)
            => property.Name;

        protected virtual object GetRawValue(object entity, IExpressionSelectorTree fields, PropertyInfo property)
            => property.GetValue(entity, null);
    }
}