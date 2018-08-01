using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RDD.Domain.Models.Querying
{
    public class QueryMetadata
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        
        public String EllapsedTime() => _stopwatch.ElapsedMilliseconds + " ms";

        public QueryMetadataPaging Paging { get; set; }

        public int TotalCount { get; set; }

        internal void StartWatch()
        {
            _stopwatch.Start();
        }
    }

    public class QueryMetadataPaging
    {
        public int PageOffset { get; set; }
        public int ItemPerPage { get; set; }
    }
}
