using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Serialization.Reflection;
using Rdd.Web.Serialization.UrlProviders;
using System;
using System.Linq;

namespace Rdd.Web.Serialization.Serializers
{
    public class BaseClassSerializer : EntitySerializer
    {
        private readonly IExpressionParser _expressionParser;

        public BaseClassSerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider, NamingStrategy namingStrategy, IUrlProvider urlProvider, IExpressionParser expressionParser, Type workingType)
            : base(serializerProvider, reflectionProvider, namingStrategy, urlProvider, workingType)
        {
            _expressionParser = expressionParser;
        }

        protected override IExpressionTree CorrectFields(object entity, IExpressionTree fields)
        {
            var fullFields = string.Join(",", ReflectionProvider.GetProperties(entity.GetType()).Select(p => p.Name));
            return _expressionParser.ParseTree(entity.GetType(), fullFields);
        }
    }
}