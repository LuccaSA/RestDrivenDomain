using System.Collections.Generic;

namespace RDD.Domain.Json
{
    public interface IJsonElement
    {
        object GetContent();

        HashSet<string> GetPaths();

        JsonArray GetJsonArray(string path);
        JsonArray GetJsonArray(Queue<string> path);

        JsonObject GetJsonObject(string path);
        JsonObject GetJsonObject(Queue<string> path);

        string GetJsonValue(string path);
        string GetJsonValue(Queue<string> path);

        bool HasJsonArray(string path);
        bool HasJsonArray(Queue<string> path);

        bool HasJsonObject(string path);
        bool HasJsonObject(Queue<string> path);

        bool HasJsonValue(string path);
        bool HasJsonValue(Queue<string> path);

        bool HasKey(string path);
        bool HasKey(Queue<string> path);
    }
}