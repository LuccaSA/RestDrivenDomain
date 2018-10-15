using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NExtends.Primitives.DateTimes;
using System;

namespace Rdd.Domain.Json
{
    public class JsonParser : IJsonParser
    {
        public IJsonElement ParseFromAnonymous(object input)
            => Parse(JObject.FromObject(input));

        public IJsonElement Parse(string input)
            => Parse((JToken)JsonConvert.DeserializeObject(input));

        public IJsonElement Parse(JToken input)
        {
            if (input == null)
            {
                return null;
            }

            switch (input.Type)
            {
                case JTokenType.Array: return ParseArray((JArray)input);
                //case JTokenType.Comment: 
                case JTokenType.Uri:
                case JTokenType.Undefined:
                case JTokenType.TimeSpan:
                case JTokenType.String:
                case JTokenType.Raw:
                case JTokenType.Property:
                case JTokenType.Null:
                case JTokenType.None:
                case JTokenType.Integer:
                case JTokenType.Guid:
                case JTokenType.Float:
                case JTokenType.Boolean:
                case JTokenType.Bytes: return ParseValue(input);
                case JTokenType.Date: return ParseDate((DateTime)input);
                case JTokenType.Object: return ParseObject((JObject)input);
            }

            return null;
        }

        protected JsonArray ParseArray(JArray array)
        {
            var result = new JsonArray();
            foreach (var token in array)
            {
                result.Content.Add(Parse(token));
            }
            return result;
        }

        protected IJsonElement ParseObject(JObject obj)
        {
            var result = new JsonObject();
            foreach (var kvp in obj)
            {
                result.Content.Add(kvp.Key.ToString(), Parse(kvp.Value));
            }
            return result;
        }

        protected IJsonElement ParseValue(JToken value) => new JsonValue { Content = value.Value<string>() };

        protected IJsonElement ParseDate(DateTime date) => new JsonValue { Content = date.ToISOz() };
    }
}