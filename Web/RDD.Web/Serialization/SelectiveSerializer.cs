using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace RDD.Web.Serialization
{
    public static class SelectiveSerializer
    {
        /// <summary>
        /// Let serialize an objet by defining wanted properties via a Node instance
        /// </summary>
        /// <param name="value"></param>
        /// <param name="settings"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string Serialize(object value, JsonSerializerSettings settings, Node node)
        {
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);

            StringBuilder sb = new StringBuilder(256);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);

            using (TrackedJsonTextWriter jsonWriter = new TrackedJsonTextWriter(sw, node))
            {
                jsonWriter.Formatting = jsonSerializer.Formatting;

                jsonSerializer.Serialize(jsonWriter, value);
            }

            return sw.ToString();
        }
    }
}