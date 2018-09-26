using System;

namespace RDD.Web.Models
{
    public class MetaHeader
    { 
        public String Generated { get; set; }
        public string Principal { get; set; }
    }

    public class MetaHeaderWithPaging : MetaHeader
    {
        public MetaPaging Paging { get; set; }
    }
}