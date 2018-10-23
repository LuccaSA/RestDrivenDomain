using NExtends.Primitives.Generics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdd.Domain.Json
{
    public class JsonObject : JsonElement
    {
        public Dictionary<string, IJsonElement> Content { get; set; }

        public JsonObject() : this(new Dictionary<string, IJsonElement>(StringComparer.OrdinalIgnoreCase)) { }

        public JsonObject(string key, IJsonElement element)
        {
            Content = new Dictionary<string, IJsonElement>(StringComparer.OrdinalIgnoreCase);
            Content[key] = element;
        }

        public JsonObject(Dictionary<string, string> data) : this(data.ToDictionary(kvp => kvp.Key, kvp => (IJsonElement)new JsonValue { Content = kvp.Value }, StringComparer.OrdinalIgnoreCase)) { }
        public JsonObject(Dictionary<string, IJsonElement> content)
        {
            Content = content;
        }

        public bool IsEmpty()
        {
            return Content.Count == 0;
        }

        public override object GetContent()
        {
            return Content.ToDictionary(p => p.Key, p => p.Value?.GetContent());
        }

        public override HashSet<string> GetPaths()
        {
            return Content.SelectMany(kvp => kvp.Value.GetPaths().Select(p => kvp.Key + (!string.IsNullOrEmpty(p) ? "." + p : ""))).ToHashSet();
        }

        public override JsonArray GetJsonArray(Queue<string> path)
        {
            if (path == null || path.Count == 0)
            {
                throw new ArgumentException("The suggested empty json path does not exist on this json object");
            }

            var currentPath = path.Dequeue();
            if (!Content.ContainsKey(currentPath))
            {
                throw new ArgumentException($"The json path '{currentPath}' does not exist on this json object");
            }

            return Content[currentPath].GetJsonArray(path);
        }

        public override JsonObject GetJsonObject(Queue<string> path)
        {
            if (path == null || path.Count == 0)
            {
                return this;
            }

            var currentPath = path.Dequeue();
            if (!Content.ContainsKey(currentPath))
            {
                throw new ArgumentException($"The json path '{currentPath}' does not exist on this json object");
            }

            return Content[currentPath].GetJsonObject(path);
        }

        public override string GetJsonValue(Queue<string> path)
        {
            if (path == null || path.Count == 0)
            {
                throw new ArgumentException("The suggested empty json path does not exist on this json object");
            }

            var currentPath = path.Dequeue();
            if (!Content.ContainsKey(currentPath))
            {
                throw new ArgumentException($"The json path '{currentPath}' does not exist on this json object");
            }

            return Content[currentPath].GetJsonValue(path);
        }

        public override bool HasJsonArray(Queue<string> path)
        {
            if (path == null || path.Count == 0)
            {
                return false;
            }

            var currentPath = path.Dequeue();
            if (!Content.ContainsKey(currentPath))
            {
                return false;
            }

            return Content[currentPath].HasJsonArray(path);
        }

        public override bool HasJsonObject(Queue<string> path)
        {
            if (path == null || path.Count == 0)
            {
                return true;
            }

            var currentPath = path.Dequeue();
            if (!Content.ContainsKey(currentPath))
            {
                return false;
            }

            return Content[currentPath].HasJsonObject(path);
        }

        public override bool HasJsonValue(Queue<string> path)
        {
            if (path == null || path.Count == 0)
            {
                return false;
            }

            var currentPath = path.Dequeue();
            if (!Content.ContainsKey(currentPath))
            {
                return false;
            }

            return Content[currentPath].HasJsonValue(path);
        }

        public override bool HasKey(Queue<string> path)
        {
            if (path == null || path.Count == 0)
            {
                return true;
            }

            var currentPath = path.Dequeue();
            if (!Content.ContainsKey(currentPath))
            {
                return false;
            }

            return Content[currentPath].HasKey(path);
        }
    }
}