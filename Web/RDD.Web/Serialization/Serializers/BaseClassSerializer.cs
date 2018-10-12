using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Web.Querying;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Serialization.UrlProviders;

namespace Rdd.Web.Serialization.Serializers
{
    public class BaseClassSerializer : EntitySerializer
    {
        public BaseClassSerializer(ISerializerProvider serializerProvider, IReflectionHelper reflectionHelper, NamingStrategy namingStrategy, IUrlProvider urlProvider)
            : base(serializerProvider, reflectionHelper, namingStrategy, urlProvider) { }

        protected override IExpressionTree CorrectFields(object entity, IExpressionTree fields)
        {
            return new FieldsParser().ParseAllProperties(entity.GetType());
        }
    }
}