using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Providers;
using System.Linq;
using System.Threading.Tasks;

namespace Rdd.Web.Serialization.Serializers
{
    public class SelectionSerializer : ObjectSerializer
    {
        public SelectionSerializer(ISerializerProvider serializerProvider, NamingStrategy namingStrategy)
            : base(serializerProvider, namingStrategy) { }

        public override Task WriteJsonAsync(JsonTextWriter writer, object entity, IExpressionTree fields)
            => WriteJsonAsync(writer, entity as ISelection, fields);

        protected async Task WriteJsonAsync(JsonTextWriter writer, ISelection selection, IExpressionTree fields)
        {
            var countField = fields.Children.FirstOrDefault(c => c.Node.Name == nameof(ISelection.Count) && typeof(ISelection).IsAssignableFrom(c.Node.ToLambdaExpression().Parameters[0].Type));

            var normalFields = new ExpressionTree
            {
                Node = fields.Node,
                Children = fields.Children.Where(c => c != countField).ToList()
            };

            await writer.WriteStartObjectAsync();

            if (countField == null || normalFields.Children.Count != 0)
            {
                var items = selection.GetItems();
                await WriteKvpAsync(writer, NamingStrategy.GetPropertyName("items", false), items, normalFields, null);
            }

            if (countField != null)
            {
                await SerializePropertyAsync(writer, selection, countField);
            }

            await writer.WriteEndObjectAsync();
        }
    }
}