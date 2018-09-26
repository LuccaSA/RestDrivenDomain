namespace RDD.Web.Querying
{
    public class QueryParsers
    {
        public QueryParsers(IWebFilterParser webFilterParser, IPagingParser pagingParser, IHeaderParser headerParser, IOrderByParser orderByParser, IFieldsParser fieldsParser)
        {
            WebFilterParser = webFilterParser;
            PagingParser = pagingParser;
            HeaderParser = headerParser;
            OrderByParser = orderByParser;
            FieldsParser = fieldsParser;
        }

        internal IWebFilterParser WebFilterParser { get; }
        internal IPagingParser PagingParser { get; }
        internal IHeaderParser HeaderParser { get; }
        internal IOrderByParser OrderByParser { get; }
        internal IFieldsParser FieldsParser { get; }
    }
}