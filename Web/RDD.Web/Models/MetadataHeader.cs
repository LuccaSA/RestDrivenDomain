using RDD.Domain;
using System;

namespace RDD.Web.Models
{
    public class MetadataHeader
    {
        public DateTime Generated { get; set; }
        public string Principal { get; set; }
        public MetadataPaging Paging { get; set; }

        public MetadataHeader(IPrincipal principal)
        {
            Principal = principal?.Name;
        }
    }

}
