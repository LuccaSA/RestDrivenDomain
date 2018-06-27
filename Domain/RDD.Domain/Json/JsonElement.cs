using System;
using System.Collections.Generic;

namespace RDD.Domain.Json
{
    public abstract class JsonElement : IJsonElement
	{
		public abstract object GetContent();

		public abstract HashSet<string> GetPaths();
		public abstract IJsonElement Map(Func<object, object> mapper);

		public JsonArray GetJsonArray(string path) => GetJsonArray(ParsePath(path));
		public abstract JsonArray GetJsonArray(Queue<string> path);

		public JsonObject GetJsonObject(string path) => GetJsonObject(ParsePath(path));
		public abstract JsonObject GetJsonObject(Queue<string> path);

		public string GetJsonValue(string path) => GetJsonValue(ParsePath(path));
		public abstract string GetJsonValue(Queue<string> path);

		public bool HasJsonArray(string path) => HasJsonArray(ParsePath(path));
		public abstract bool HasJsonArray(Queue<string> path);

		public bool HasJsonObject(string path) => HasJsonObject(ParsePath(path));
		public abstract bool HasJsonObject(Queue<string> path);

		public bool HasJsonValue(string path) => HasJsonValue(ParsePath(path));
		public abstract bool HasJsonValue(Queue<string> path);

		public bool HasKey(string path) => HasKey(ParsePath(path));

		public abstract bool HasKey(Queue<string> path);

		public bool RemovePath(string path) { return RemovePath(ParsePath(path)); }
		public abstract bool RemovePath(Queue<string> path);

		Queue<string> ParsePath(string path)
		{
			return new Queue<string>((path ?? "").Replace("]", "").Split(new[] { '.', '[' }, StringSplitOptions.RemoveEmptyEntries));
		}
	}
}