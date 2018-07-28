using System;
using System.Diagnostics;

namespace RDD.Domain.Models.Querying
{
    public class QueryMetadata
    {
        public QueryMetadata()
        {
            ;
        }

        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        
        public String EllapsedTime() => _stopwatch.ElapsedMilliseconds + " ms";

        public QueryMetadataPaging Paging { get; set; }

        public int TotalCount { get; set; }
    }

    public class QueryMetadataPaging
    {
        public int PageOffset { get; set; }
        public int ItemPerPage { get; set; }
    }
}