using System;
using System.Collections.Generic;

namespace RDD.Domain.Json
{
    public class JsonValue : JsonElement
    {
        public object Content { get; set; }

        public JsonValue() { }
        public JsonValue(object value)
        {
            Content = value;
        }

        public override object GetContent()
        {
            return Content;
        }

        public override HashSet<string> GetPaths()
        {
            return new HashSet<string> { "" };
        }

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
            if (path == null || path.Count == 0)
                return Content == null ? null : Content.ToString();

            throw new ArgumentException($"The json path '{string.Join(".", path)}' does not exist on this json value");
        }

        public override bool HasJsonArray(Queue<string> path)
        {
            return false;
        }

        public override bool HasJsonObject(Queue<string> path)
        {
            return false;
        }

        public override bool HasJsonValue(Queue<string> path)
        {
            return path == null || path.Count == 0;
        }

        public override bool HasKey(Queue<string> path) => HasJsonValue(path);
    }
}