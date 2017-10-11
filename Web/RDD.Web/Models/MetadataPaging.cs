using Newtonsoft.Json;

namespace RDD.Web.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MetadataPaging
    {
        [JsonProperty]
        public int count { get; set; }

        [JsonProperty]
        public int offset { get; set; }
        [JsonProperty]
        public int limit { get; set; }

        [JsonProperty]
        public string previous { get; set; }
        [JsonProperty]
        public string next { get; set; }
    }
}
