using Newtonsoft.Json;
using RDD.Domain.Helpers.Expressions;

namespace RDD.Web.Serialization.Serializers
{
    public interface ISerializer
    {
        void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields);
    }
}