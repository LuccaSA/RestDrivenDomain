using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Web.Serialization.Providers;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rdd.Web.Serialization.Serializers
{
    public class ObjectSerializer : ISerializer
    {
        protected ISerializerProvider SerializerProvider { get; private set; }
        protected IReflectionHelper ReflectionHelper { get; private set; }
        protected NamingStrategy NamingStrategy { get; private set; }

        public ObjectSerializer(ISerializerProvider serializerProvider, IReflectionHelper reflectionHelper, NamingStrategy namingStrategy)
        {
            SerializerProvider = serializerProvider ?? throw new ArgumentNullException(nameof(serializerProvider));
            ReflectionHelper = reflectionHelper ?? throw new ArgumentNullException(nameof(reflectionHelper));
            NamingStrategy = namingStrategy ?? throw new ArgumentNullException(nameof(namingStrategy));
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
                var type = entity.GetType();
                return new ExpressionParser().ParseTree(entity.GetType(), string.Join(",", ReflectionHelper.GetProperties(type).Select(p => p.Name)));
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
            => ReflectionHelper.GetValue(entity, property);
    }
}