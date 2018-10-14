using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdd.Web.Serialization.Serializers
{
    public class ObjectSerializer : ISerializer
    {
        protected ISerializerProvider SerializerProvider { get; private set; }
        protected NamingStrategy NamingStrategy { get; private set; }
        protected Dictionary<Type, IExpressionTree> DefaultFields { get; set; }

        public ObjectSerializer(ISerializerProvider serializerProvider, NamingStrategy namingStrategy)
        {
            SerializerProvider = serializerProvider ?? throw new ArgumentNullException(nameof(serializerProvider));
            NamingStrategy = namingStrategy ?? throw new ArgumentNullException(nameof(namingStrategy));
            DefaultFields = new Dictionary<Type, IExpressionTree>();
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
            if (fields == null || fields.Children.Count == 0)
            {
                var type = entity.GetType();
                if (!DefaultFields.ContainsKey(type))
                {
                    DefaultFields[type] = new ExpressionParser().ParseTree(type, string.Join(",", type.GetProperties().Select(p => p.Name)));
                }

                return DefaultFields[type];
            }

            return fields;
        }

        protected virtual void SerializeProperty(JsonTextWriter writer, object entity, IExpressionTree fields)
        {
            SerializeProperty(writer, entity, fields, fields.Node as PropertyExpression);
        }

        protected virtual void SerializeProperty(JsonTextWriter writer, object entity, IExpressionTree fields, PropertyExpression property)
        {
            WriteKvp(writer, GetKey(entity, fields, property), GetRawValue(entity, fields, property), fields, property);
        }

        protected virtual void WriteKvp(JsonTextWriter writer, string key, object value, IExpressionTree fields, PropertyExpression property)
        {
            writer.WritePropertyName(key, true);
            SerializerProvider.ResolveSerializer(value).WriteJson(writer, value, fields);
        }

        protected virtual string GetKey(object entity, IExpressionTree fields, PropertyExpression property)
            => NamingStrategy.GetPropertyName(property.Name, false);

        protected virtual object GetRawValue(object entity, IExpressionTree fields, PropertyExpression property)
            => property.ValueProvider.GetValue(entity);
    }
}