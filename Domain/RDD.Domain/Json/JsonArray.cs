using NExtends.Primitives.Generics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Domain.Json
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
            return Content.Select(e => e == null ? null : e.GetContent()).ToArray();
        }

        public override IJsonElement Map(Func<object, object> mapper)
        {
            if (Content == null)
                return new JsonArray();

            return new JsonArray(Content.Select(e => e == null ? null : e.Map(mapper)));
        }

        public override HashSet<string> GetPaths()
        {
            return Content.SelectMany((e, index) => e.GetPaths().Select(p => "[" + index + "]" + p)).ToHashSet();
        }

        public List<string> GetEveryJsonValue()
        {
            return GetEveryJsonValue(null);
        }

        public List<string> GetEveryJsonValue(string path)
        {
            if (Content == null)
                return null;

            return Content.Select(e => e.GetJsonValue(path)).ToList();
        }

        public bool HasEveryJsonValue(string path)
        {
            if (Content == null || Content.Count == 0)
                return false;

            return Content.All(e => e.HasJsonValue(path));
        }

        public override JsonArray GetJsonArray(Queue<string> path)
        {
            if (path == null || path.Count == 0)
                return this;

            var currentPath = path.Dequeue();
            int index;
            if (!int.TryParse(currentPath, out index))
                throw new ArgumentException("Path on a json array must be a valid integer");

            return Content[index].GetJsonArray(path);
        }

        public override JsonObject GetJsonObject(Queue<string> path)
        {
            if (path == null || path.Count == 0)
                throw new ArgumentException("The suggested empty json path does not exist on this json array");

            var currentPath = path.Dequeue();
            int index;
            if (!int.TryParse(currentPath, out index))
                throw new ArgumentException("Path on a json array must be a valid integer");

            return Content[index].GetJsonObject(path);
        }

        public override string GetJsonValue(Queue<string> path)
        {
            if (path == null || path.Count == 0)
                throw new ArgumentException("The suggested empty json path does not exist on this json array");

            var currentPath = path.Dequeue();
            int index;
            if (!int.TryParse(currentPath, out index))
                throw new ArgumentException("Path on a json array must be a valid integer");

            return Content[index].GetJsonValue(path);
        }

        public override bool HasJsonArray(Queue<string> path)
        {
            if (path == null || path.Count == 0)
                return true;

            var currentPath = path.Dequeue();
            int index;
            if (!int.TryParse(currentPath, out index))
                throw new ArgumentException("Path on a json array must be a valid integer");

            return Content[index].HasJsonArray(path);
        }

        public override bool HasJsonObject(Queue<string> path)
        {
            if (path == null || path.Count == 0)
                return false;

            var currentPath = path.Dequeue();
            int index;
            if (!int.TryParse(currentPath, out index))
                return false;

            return Content[index].HasJsonObject(path);
        }

        public override bool HasJsonValue(Queue<string> path)
        {
            if (path == null || path.Count == 0)
                return false;

            var currentPath = path.Dequeue();
            int index;
            if (!int.TryParse(currentPath, out index))
                return false;

            return Content[index].HasJsonValue(path);
        }

        public override bool HasKey(Queue<string> path)
        {
            if (path == null || path.Count == 0)
            {
                return true;
            }

            var currentPath = path.Dequeue();
            if (!int.TryParse(currentPath, out var index))
            {
                return false;
            }

            return Content[index].HasKey(path);
        }

        public override bool RemovePath(Queue<string> path)
        {
            if (path == null || path.Count == 0)
                return false;

            var currentPath = path.Dequeue();
            int index;
            if (!int.TryParse(currentPath, out index))
                return false;

            if (path.Count == 0)
            {
                Content.RemoveAt(index);
                return true;
            }

            return Content[index].RemovePath(path);
        }
    }
}