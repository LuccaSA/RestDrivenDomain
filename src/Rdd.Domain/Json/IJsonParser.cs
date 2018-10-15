using Newtonsoft.Json.Linq;

namespace Rdd.Domain.Json
{
    public interface IJsonParser
    {
        IJsonElement Parse(string input);
        IJsonElement Parse(JToken input);
        IJsonElement ParseFromAnonymous(object input);
    }
}