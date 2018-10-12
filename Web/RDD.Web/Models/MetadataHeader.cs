using Rdd.Domain;
using System;

namespace Rdd.Web.Models
{
    public class MetadataHeader
    {
        public DateTime Generated { get; set; }
        public string Principal { get; set; }

        public MetadataHeader(IPrincipal principal)
        {
            Principal = principal?.Name;
        }
    }

}
