using RDD.Domain;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using RDD.Web.Serialization.Providers;
using RDD.Web.Serialization.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Web.Serialization.Serializers
{
    public class SelectionSerializer : ObjectSerializer
    {
        public SelectionSerializer(ISerializerProvider serializerProvider, IReflectionProvider reflectionProvider)
            : base(serializerProvider, reflectionProvider, typeof(ISelection)) { }

        public override IJsonElement ToJson(object entity, IExpressionSelectorTree fields)
        {
            return ToSerializableObject(entity as ISelection, fields);
        }

        protected IJsonElement ToSerializableObject(ISelection selection, IExpressionSelectorTree fields)
        {
            var items = selection.GetItems();

            var countField = fields.Children.FirstOrDefault(c => c.Node.Name == nameof(ISelection.Count) && typeof(ISelection).IsAssignableFrom(c.Node.ToLambdaExpression().Parameters[0].Type));

            var normalFields = new ExpressionSelectorTree
            {
                Node = fields.Node,
                Children = fields.Children.Where(c => c != countField).ToList()
            };

            var dico = new Dictionary<string, IJsonElement>
            {
                { "items", SerializerProvider.GetSerializer(items).ToJson(items, normalFields) }
            };

            var result = new JsonObject { Content = dico };

            if (countField != null)
            {
                SerializeProperty(result, selection, countField);
            }
            return result;
        }
    }
}