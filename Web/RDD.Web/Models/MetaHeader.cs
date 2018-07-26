using System;

namespace RDD.Web.Models
{
    public class MetaHeader
    {
        public MetaHeader()
        {
        }
 
        public String Generated { get; set; }
        public string Principal { get; set; }
        public MetaPaging Paging { get; set; }
         
    }
}