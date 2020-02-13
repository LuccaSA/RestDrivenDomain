using Newtonsoft.Json;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Web.Serialization.Serializers;
using System.Threading.Tasks;

namespace Rdd.Web.Serialization.Providers
{
    public interface ISerializerProvider
    {
        ISerializer ResolveSerializer(object entity);
    }

    public static class ISerializerProviderExtensions
    {
        public static Task WriteJsonAsync(this ISerializerProvider serializerProvider, JsonTextWriter writer, object entity, IExpressionTree fields)
            => serializerProvider.ResolveSerializer(entity).WriteJsonAsync(writer, entity, fields);
    }

}