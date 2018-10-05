using Newtonsoft.Json.Linq;

namespace RDD.Domain.Json
{
    public interface IJsonParser
    {
        IJsonElement Parse(string input);
        IJsonElement Parse(JToken input);
        IJsonElement ParseFromAnonymous(object input);
    }
}