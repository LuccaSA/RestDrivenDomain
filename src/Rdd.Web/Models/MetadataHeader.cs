using System;

namespace Rdd.Web.Models
{
    public class MetadataHeader
    {
        public DateTime Generated { get; set; }
        public string Principal { get; set; }

        public MetadataHeader(string principalName)
        {
            Principal = principalName;
        }
    }
}