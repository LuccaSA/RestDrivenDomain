using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Querying;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Serialization.UrlProviders;

namespace Rdd.Web.Serialization.Serializers
{
    public class BaseClassSerializer : EntitySerializer
    {
        private readonly IFieldsParser _fieldsParser;

        public BaseClassSerializer(ISerializerProvider serializerProvider, IFieldsParser fieldsParser, NamingStrategy namingStrategy, IUrlProvider urlProvider)
            : base(serializerProvider, namingStrategy, urlProvider)
        {
            _fieldsParser = fieldsParser;
        }

        protected override IExpressionTree CorrectFields(object entity, IExpressionTree fields)
            => DefaultFields.GetOrAdd(entity.GetType(), t => _fieldsParser.ParseDefaultFields(t));
    }
}