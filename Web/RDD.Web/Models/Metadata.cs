using RDD.Domain;
using System;

namespace RDD.Web.Models
{
    public class Metadata
    {
        public MetadataHeader Header { get; set; }
        public object Data { get; set; }

        public Metadata(object datas, IPrincipal principal, DateTime generatedAt)
        {
            Header = new MetadataHeader(principal) { Generated = generatedAt };
            Data = datas;
        }
    }
}