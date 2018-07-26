using System;
using System.Diagnostics;

namespace RDD.Domain.Models.Querying
{
    public class QueryContext
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
       
        public QueryContext(QueryRequest request, QueryResponse response)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Response = response ?? throw new ArgumentNullException(nameof(response));
        }

        public String EllapsedTime() => _stopwatch.ElapsedMilliseconds + " ms";

        public QueryRequest Request { get; }
        public QueryResponse Response { get; }
    }
}