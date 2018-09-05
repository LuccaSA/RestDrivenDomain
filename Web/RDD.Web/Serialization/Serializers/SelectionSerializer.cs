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
            : base( serializerProvider,  reflectionProvider, typeof(ISelection)) { }

        public override IJsonElement ToJson(object entity, IExpressionSelectorTree fields)
        {
            return ToSerializableObject(entity as ISelection, fields);
        }

        protected IJsonElement ToSerializableObject(ISelection selection, IExpressionSelectorTree fields)
        {
            var items = selection.GetItems();

            var normalFields = new ExpressionSelectorTree
            {
                Node = fields.Node,
                Children = fields.Children.Where(c => !(c.Node is MethodCallSelector) && !typeof(ISelection).IsAssignableFrom(c.Node.ToLambdaExpression().Parameters[0].Type)).ToList()
            };

            var dico = new Dictionary<string, IJsonElement>
            {
                { "items", SerializerProvider.GetSerializer(items).ToJson(items, normalFields) }
            };

            foreach (var method in new[] { nameof(ISelection.Sum), nameof(ISelection.Max), nameof(ISelection.Min) })
            {
                AddMethodResults(method, selection, fields, dico);
            }

            var result = new JsonObject { Content = dico };

            var countField = fields.Children.Where(c => c.Node.Name == nameof(ISelection.Count)).FirstOrDefault();
            if (countField != null)
            {
                SerializeProperty(result, selection, countField);
            }
            return result;
        }

        private void AddMethodResults(string methodName, ISelection collection, IExpressionSelectorTree fields, Dictionary<string, IJsonElement> dico)
        {
            var methodFields = fields.Children.Select(t => t.Node as MethodCallSelector).Where(n => n != null && n.Name == methodName).ToList();

            if (methodFields.Any())
            {
                var value = methodFields.ToDictionary(
                    m => ((m.MethodCallExpression.Arguments[0] as ConstantExpression).Value as PropertyInfo).Name, 
                    m => (IJsonElement)new JsonValue(m.ToLambdaExpression().Compile().DynamicInvoke(collection))
                );

                dico.Add(methodName.ToLower() + "s", new JsonObject(value));
            }
        }
    }
}