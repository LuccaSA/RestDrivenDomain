using Newtonsoft.Json;
using Rdd.Domain.Helpers.Expressions;
using System.Threading.Tasks;

namespace Rdd.Web.Serialization.Serializers
{
    public class ToStringSerializer : ISerializer
    {
        public Task WriteJsonAsync(JsonTextWriter writer, object entity, IExpressionTree fields)
            => writer.WriteValueAsync(entity.ToString());
    }
}