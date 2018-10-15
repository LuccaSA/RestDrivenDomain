using System;

namespace Rdd.Web.Models
{
    public class Metadata
    {
        public MetadataHeader Header { get; set; }
        public object Data { get; set; }

        public Metadata(object datas, string principalName, DateTime generatedAt)
        {
            Header = new MetadataHeader(principalName) { Generated = generatedAt };
            Data = datas;
        }
    }
}