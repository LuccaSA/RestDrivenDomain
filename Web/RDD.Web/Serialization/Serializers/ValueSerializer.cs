using Newtonsoft.Json;
using Rdd.Domain.Helpers.Expressions;

namespace Rdd.Web.Serialization.Serializers
{
    public class ValueSerializer : ISerializer
    {
        public virtual void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields)
            => writer.WriteValue(entity);
    }
}