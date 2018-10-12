using Newtonsoft.Json.Serialization;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Serialization.Reflection;
using Rdd.Web.Serialization.UrlProviders;
using System;
using System.Linq;
using System.Reflection;

namespace Rdd.Web.Serialization.Serializers
{
    public class EntitySerializer : ObjectSerializer
    {
        protected IUrlProvider UrlProvider { get; set; }

        public EntitySerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider, NamingStrategy namingStrategy, IUrlProvider urlProvider, Type workingType)
            : base(serializerProvider, reflectionProvider, namingStrategy, workingType)
        {
            UrlProvider = urlProvider;
        }

        protected override IExpressionTree CorrectFields(object entity, IExpressionTree fields)
        {
            if (fields == null || !fields.Children.Any())
            {
                return new ExpressionParser().ParseTree(entity.GetType(), "id,name,url");
            }

            return base.CorrectFields(entity, fields);
        }

        protected override object GetRawValue(object entity, IExpressionTree fields, PropertyInfo property)
        {
            if (property.Name == "Url")
            {
                return UrlProvider?.GetEntityApiUri(WorkingType, entity as IPrimaryKey);
            }

            return base.GetRawValue(entity, fields, property);
        }
    }
}