using RDD.Domain;
using RDD.Domain.Helpers.Expressions;
using RDD.Web.Querying;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using RDD.Web.Serialization.UrlProviders;
using System;
using System.Linq;
using System.Reflection;

namespace RDD.Web.Serialization.Serializers
{
    public class EntitySerializer : ObjectSerializer
    {
        protected IUrlProvider UrlProvider { get; set; }

        public EntitySerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider, IUrlProvider urlProvider, Type workingType)
            : base(serializerProvider, reflectionProvider, workingType)
        {
            UrlProvider = urlProvider;
        }

        protected override IExpressionSelectorTree CorrectFields(object entity, IExpressionSelectorTree fields)
        {
            if (fields == null || !fields.Children.Any())
            {
                return new ExpressionSelectorParser().ParseTree(entity.GetType(), "id,name,url");
            }

            return base.CorrectFields(entity, fields);
        }

        protected override object GetRawValue(object entity, IExpressionSelectorTree fields, PropertyInfo property)
        {
            if (property.Name == "Url")
            {
                return UrlProvider?.GetEntityApiUri(WorkingType, entity as IPrimaryKey);
            }

            return base.GetRawValue(entity, fields, property);
        }
    }
}