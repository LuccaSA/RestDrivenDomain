using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Querying;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Serialization.Reflection;
using Rdd.Web.Serialization.UrlProviders;
using System;

namespace Rdd.Web.Serialization.Serializers
{
    public class BaseClassSerializer : EntitySerializer
    {
        public BaseClassSerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider, NamingStrategy namingStrategy, IUrlProvider urlProvider, Type workingType)
            : base(serializerProvider, reflectionProvider, namingStrategy, urlProvider, workingType) { }

        protected override IExpressionTree CorrectFields(object entity, IExpressionTree fields)
        {
            return new FieldsParser().ParseAllProperties(entity.GetType());
        }
    }
}