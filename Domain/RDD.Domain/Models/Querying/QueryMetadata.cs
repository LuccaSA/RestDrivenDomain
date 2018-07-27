using System;
using System.Diagnostics;

namespace RDD.Domain.Models.Querying
{
    public class QueryMetadata
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        
        public String EllapsedTime() => _stopwatch.ElapsedMilliseconds + " ms";
        public QueryPaging Paging { get; set; }
        public int TotalCount { get; set; }
    }
}