using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Querying;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Serialization.UrlProviders;

namespace Rdd.Web.Serialization.Serializers
{
    public class BaseClassSerializer : EntitySerializer
    {
        public BaseClassSerializer(ISerializerProvider serializerProvider, NamingStrategy namingStrategy, IUrlProvider urlProvider)
            : base(serializerProvider, namingStrategy, urlProvider)
        {
        }

        protected override IExpressionTree CorrectFields(object entity, IExpressionTree fields)
        {
            var type = entity.GetType();
            if (!DefaultFields.ContainsKey(type))
            {
                DefaultFields[type] = new FieldsParser().ParseAllProperties(type);
            }

            return DefaultFields[type];
        }
    }
}