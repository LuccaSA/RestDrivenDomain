using Newtonsoft.Json;
using RDD.Domain.Helpers.Expressions;

namespace RDD.Web.Serialization.Serializers
{
    public class ToStringSerializer : ISerializer
    {
        public void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields)
            => writer.WriteValue(entity.ToString());
    }
}