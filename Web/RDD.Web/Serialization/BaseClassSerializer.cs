using RDD.Domain.Helpers.Expressions;
using RDD.Web.Querying;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using RDD.Web.Serialization.Serializers;
using RDD.Web.Serialization.UrlProviders;
using System;

namespace RDD.Web.Serialization
{
    public class BaseClassSerializer : EntitySerializer
    {
        public BaseClassSerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider, IUrlProvider urlProvider, Type workingType)
            : base(serializerProvider, reflectionProvider, urlProvider, workingType) { }

        protected override IExpressionTree CorrectFields(object entity, IExpressionTree fields)
        {
            return new FieldsParser().ParseAllProperties(WorkingType);
        }
    }
}