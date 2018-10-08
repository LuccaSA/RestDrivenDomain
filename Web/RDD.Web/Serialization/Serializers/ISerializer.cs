using Newtonsoft.Json;
using Rdd.Domain.Helpers.Expressions;

namespace Rdd.Web.Serialization.Serializers
{
    public interface ISerializer
    {
        void WriteJson(JsonTextWriter writer, object entity, IExpressionTree fields);
    }
}