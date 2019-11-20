using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdd.Domain.Json
{
    public class JsonArray : JsonElement
    {
        public List<IJsonElement> Content { get; set; }

        public JsonArray() : this(new List<IJsonElement>()) { }
        public JsonArray(IEnumerable<IJsonElement> elements)
        {
            Content = elements.ToList();
        }

        public override object GetContent()
        {
            return Content.Select(e => e?.GetContent()).ToArray();
        }

        public override HashSet<string> GetPaths()
        {
            return Content.SelectMany((e, index) => e.GetPaths().Select(p => index + "." + p)).ToHashSet();
        }

        public List<string> GetEveryJsonValue()
        {
            return GetEveryJsonValue(null);
        }

        public List<string> GetEveryJsonValue(string path)
        {
            if (Content == null)
            {
                return null;
            }

            return Content.Select(e => e.GetJsonValue(path)).ToList();
        }

        public bool HasEveryJsonValue(string path)
        {
            if (Content == null || Content.Count == 0)
            {
                return false;
            }

            return Content.All(e => e.HasJsonValue(path));
        }

        public override JsonArray GetJsonArray(Queue<string> path)
        {
            if (path == null || path.Count == 0)
            {
                return this;
            }

            var currentPath = path.Dequeue();
            if (int.TryParse(currentPath, out var index) && 0 <= index && index < Content.Count)
            {
                return Content[index].GetJsonArray(path);
            }
            throw new ArgumentException("Path on a json array must be a valid integer");
        }

        public override JsonObject GetJsonObject(Queue<string> path)
        {
            if (path == null || path.Count == 0)
            {
                throw new ArgumentException("The suggested empty json path does not exist on this json array");
            }

            var currentPath = path.Dequeue();
            if (int.TryParse(currentPath, out var index) && 0 <= index && index < Content.Count)
            {
                return Content[index].GetJsonObject(path);
            }
            throw new ArgumentException("Path on a json array must be a valid integer");
        }

        public override string GetJsonValue(Queue<string> path)
        {
            if (path == null || path.Count == 0)
            {
                throw new ArgumentException("The suggested empty json path does not exist on this json array");
            }

            var currentPath = path.Dequeue();
            if (int.TryParse(currentPath, out var index) && 0 <= index && index < Content.Count)
            {
                return Content[index].GetJsonValue(path);
            }
            throw new ArgumentException("Path on a json array must be a valid integer");
        }

        public override bool HasJsonArray(Queue<string> path) => path == null || path.Count == 0 ||
        (
            int.TryParse(path.Dequeue(), out var index)
            && 0 <= index && index < Content.Count
            && Content[index].HasJsonArray(path)
        );

        public override bool HasJsonObject(Queue<string> path) =>
            path != null
            && path.Count != 0
            && int.TryParse(path.Dequeue(), out var index)
            && 0 <= index && index < Content.Count
            && Content[index].HasJsonObject(path);

        public override bool HasJsonValue(Queue<string> path) =>
            path != null
            && path.Count != 0
            && int.TryParse(path.Dequeue(), out var index)
            && 0 <= index && index<Content.Count
            && Content[index].HasJsonValue(path);

        public override bool HasKey(Queue<string> path) => path == null || path.Count == 0 ||
        (
            int.TryParse(path.Dequeue(), out var index)
            && 0 <= index && index < Content.Count
            && Content[index].HasKey(path)
        );
    }
}