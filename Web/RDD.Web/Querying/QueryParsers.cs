namespace RDD.Web.Querying
{
    public class QueryParsers
    {
        public QueryParsers(IWebFilterParser webFilterParser, IPagingParser pagingParser, IHeaderParser headerParser, IOrberByParser orderByParser)
        {
            WebFilterParser = webFilterParser;
            PagingParser = pagingParser;
            HeaderParser = headerParser;
            OrderByParser = orderByParser;
        }

        internal IWebFilterParser WebFilterParser { get; }

        internal IPagingParser PagingParser { get; }

        internal IHeaderParser HeaderParser { get; }

        internal IOrberByParser OrderByParser { get; }
    }
}