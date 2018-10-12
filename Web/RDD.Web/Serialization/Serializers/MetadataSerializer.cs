using Newtonsoft.Json.Serialization;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Models;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Serialization.Reflection;
using System.Linq;

namespace Rdd.Web.Serialization.Serializers
{
    public class MetadataSerializer : ObjectSerializer
    {
        public MetadataSerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider, NamingStrategy namingStrategy)
            : base(serializerProvider, reflectionProvider, namingStrategy) { }

        protected override IExpressionTree CorrectFields(object entity, IExpressionTree fields)
        {
            var result = new ExpressionParser().ParseTree<Metadata>("header[generated,principal],data");

            var dataNode = result.Children.First(c => c.Node.Name == "Data") as ExpressionTree;
            dataNode.Children = fields.Children.ToList();

            return result;
        }
    }
}