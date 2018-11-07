using System;
using System.Collections.Generic;

namespace Rdd.Domain.Json
{
    public class JsonValue : JsonElement
    {
        public object Content { get; set; }

        public JsonValue() { }
        public JsonValue(object value)
        {
            Content = value;
        }

        public override object GetContent() => Content;

        public override HashSet<string> GetPaths() => new HashSet<string> { "" };

        public override JsonArray GetJsonArray(Queue<string> path)
        {
            throw new ArgumentException($"The json path '{string.Join(".", path)}' does not exist on this json value");
        }

        public override JsonObject GetJsonObject(Queue<string> path)
        {
            throw new ArgumentException($"The json path '{string.Join(".", path)}' does not exist on this json value");
        }

        public override string GetJsonValue(Queue<string> path)
        {
            if (HasJsonValue(path))
            {
                return Content?.ToString();
            }

            throw new ArgumentException($"The json path '{string.Join(".", path)}' does not exist on this json value");
        }

        public override bool HasJsonArray(Queue<string> path) => false;
        public override bool HasJsonObject(Queue<string> path) => false;
        public override bool HasJsonValue(Queue<string> path) => path == null || path.Count == 0;

        public override bool HasKey(Queue<string> path) => HasJsonValue(path);
    }
}