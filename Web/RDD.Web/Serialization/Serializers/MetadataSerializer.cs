using Newtonsoft.Json.Serialization;
using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
using RDD.Web.Models;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using System.Linq;

namespace RDD.Web.Serialization.Serializers
{
    public class MetadataSerializer : ObjectSerializer
    {
        public MetadataSerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider, NamingStrategy namingStrategy)
            : base(serializerProvider, reflectionProvider, namingStrategy, typeof(Culture)) { }

        protected override IExpressionTree CorrectFields(object entity, IExpressionTree fields)
        {
            var result = new ExpressionParser().ParseTree<Metadata>("header[generated,principal],data");

            var dataNode = result.Children.First(c => c.Node.Name == "Data") as ExpressionTree;
            dataNode.Children = fields.Children.ToList();

            return result;
        }
    }
}