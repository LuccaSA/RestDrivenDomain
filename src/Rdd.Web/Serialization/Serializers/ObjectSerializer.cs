using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Rdd.Web.Serialization.Serializers
{
    public class ObjectSerializer : ISerializer
    {
        protected ISerializerProvider SerializerProvider { get; private set; }
        protected NamingStrategy NamingStrategy { get; private set; }
        protected ConcurrentDictionary<Type, IExpressionTree> DefaultFields { get; set; }

        public ObjectSerializer(ISerializerProvider serializerProvider, NamingStrategy namingStrategy)
        {
            SerializerProvider = serializerProvider ?? throw new ArgumentNullException(nameof(serializerProvider));
            NamingStrategy = namingStrategy ?? throw new ArgumentNullException(nameof(namingStrategy));
            DefaultFields = new ConcurrentDictionary<Type, IExpressionTree>();
        }

        public virtual async Task WriteJsonAsync(JsonTextWriter writer, object entity, IExpressionTree fields)
        {
            await writer.WriteStartObjectAsync();

            foreach (var subSelector in CorrectFields(entity, fields).Children)
            {
                await SerializePropertyAsync(writer, entity, subSelector);
            }

            await writer.WriteEndObjectAsync();
        }

        protected virtual IExpressionTree CorrectFields(object entity, IExpressionTree fields)
        {
            if (fields == null || fields.Children.Count == 0)
            {
                return DefaultFields.GetOrAdd(entity.GetType(), t => new ExpressionParser().ParseTree(t, string.Join(",", t.GetProperties().Select(p => p.Name))));
            }

            return fields;
        }

        protected virtual Task SerializePropertyAsync(JsonTextWriter writer, object entity, IExpressionTree fields)
        {
            return SerializePropertyAsync(writer, entity, fields, fields.Node as PropertyExpression);
        }

        protected virtual Task SerializePropertyAsync(JsonTextWriter writer, object entity, IExpressionTree fields, PropertyExpression property)
        {
            return WriteKvpAsync(writer, GetKey(entity, fields, property), GetRawValue(entity, fields, property), fields, property);
        }

        protected virtual async Task WriteKvpAsync(JsonTextWriter writer, string key, object value, IExpressionTree fields, PropertyExpression property)
        {
            await writer.WritePropertyNameAsync(key, true);
            await SerializerProvider.ResolveSerializer(value).WriteJsonAsync(writer, value, fields);
        }

        protected virtual string GetKey(object entity, IExpressionTree fields, PropertyExpression property)
            => NamingStrategy.GetPropertyName(property.Name, false);

        protected virtual object GetRawValue(object entity, IExpressionTree fields, PropertyExpression property)
            => property.ValueProvider.GetValue(entity);
    }
}