using RDD.Domain;
using System;

namespace RDD.Web.Models
{
    public class MetadataHeader
    {
        public DateTime generated { get; set; }

        public string principal { get; set; }

        public MetadataPaging Paging { get; set; }

        public MetadataHeader(IExecutionContext execution)
        {
            principal = execution.curPrincipal.Name;
        }
    }

}
