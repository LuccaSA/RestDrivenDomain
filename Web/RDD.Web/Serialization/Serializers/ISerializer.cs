using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;

namespace RDD.Web.Serialization.Serializers
{
    public interface ISerializer
    {
        IJsonElement ToJson(object entity, IExpressionSelectorTree fields);
    }
}