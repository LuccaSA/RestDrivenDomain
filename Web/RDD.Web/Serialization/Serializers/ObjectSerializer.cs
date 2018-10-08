using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Serialization.Reflection;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rdd.Web.Serialization.Serializers
{
    public class ObjectSerializer : ISerializer
    {
        protected ISerializerProvider SerializerProvider { get; private set; }
        protected IReflectionProvider ReflectionProvider { get; private set; }
        protected NamingStrategy NamingStrategy { get; private set; }

        public Type WorkingType { get; set; }

        public ObjectSerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider, NamingStrategy namingStrategy, Type workingType)
        {
            SerializerProvider = serializerProvider;
            NamingStrategy = namingStrategy;
            ReflectionProvider = reflectionProvider;
            WorkingType = workingType;
        }

        public virtual void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields)
        {
            writer.WriteStartObject();

            foreach (var subSelector in CorrectFields(entity, fields).Children)
            {
                SerializeProperty(writer, entity, subSelector);
            }

            writer.WriteEndObject();
        }

        protected virtual IExpressionTree CorrectFields(object entity, IExpressionTree fields)
        {
            if (fields == null || !fields.Children.Any())
            {
                return new ExpressionParser().ParseTree(WorkingType,
                    string.Join(",", ReflectionProvider.GetProperties(WorkingType).Select(p => p.Name)));
            }

            return fields;
        }

        protected virtual void SerializeProperty(JsonTextWriter writer, object entity, IExpressionTree fields)
        {
            var property = (fields.Node.ToLambdaExpression().Body as MemberExpression).Member as PropertyInfo;
            SerializeProperty(writer, entity, fields, property);
        }

        protected virtual void SerializeProperty(JsonTextWriter writer, object entity, IExpressionTree fields, PropertyInfo property)
        {
            var key = GetKey(entity, fields, property);
            var value = GetRawValue(entity, fields, property);

            WriteKvp(writer, key, value, fields, property);
        }

        protected virtual void WriteKvp(JsonTextWriter writer, string key, object value, IExpressionTree fields, PropertyInfo property)
        {
            writer.WritePropertyName(key, true);
            SerializerProvider.GetSerializer(value).WriteJson(writer, value, fields);
        }

        protected virtual string GetKey(object entity, IExpressionTree fields, PropertyInfo property)
            => NamingStrategy.GetPropertyName(property.Name, false);

        protected virtual object GetRawValue(object entity, IExpressionTree fields, PropertyInfo property)
            => property.GetValue(entity, null);
    }
}