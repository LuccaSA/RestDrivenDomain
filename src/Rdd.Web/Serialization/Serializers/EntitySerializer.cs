using Newtonsoft.Json.Serialization;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Serialization.UrlProviders;

namespace Rdd.Web.Serialization.Serializers
{
    public class EntitySerializer : ObjectSerializer
    {
        protected IUrlProvider UrlProvider { get; set; }

        public EntitySerializer(ISerializerProvider serializerProvider,  NamingStrategy namingStrategy, IUrlProvider urlProvider)
            : base(serializerProvider, namingStrategy)
        {
            UrlProvider = urlProvider;
        }

        protected override IExpressionTree CorrectFields(object entity, IExpressionTree fields)
        {
            if (fields == null || fields.Children.Count == 0)
            {
                return DefaultFields.GetOrAdd(entity.GetType(), t => new ExpressionParser().ParseTree(t, "id,name,url"));
            }

            return base.CorrectFields(entity, fields);
        }

        protected override object GetRawValue(object entity, IExpressionTree fields, PropertyExpression property)
        {
            if (property.Name == nameof(IEntityBase.Url))
            {
                return UrlProvider?.GetEntityApiUri(entity as IPrimaryKey);
            }

            return base.GetRawValue(entity, fields, property);
        }
    }
}