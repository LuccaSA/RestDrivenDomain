using System.Collections.Generic;

namespace RDD.Web.Middleware
{
    public class QueryTokens
    {
        /// <summary>
        /// Use for explicit field selection on the serialization step
        /// </summary>
        public const string Fields = "fields"; 
        /// <summary>
        /// Keyword for paging infos
        /// </summary>
        public const string Paging = "paging"; 
        /// <summary>
        /// Used to defined query order clauses 
        /// </summary>
        public const string OrderBy = "orderby";

        public static readonly HashSet<string> Reserved = new HashSet<string>
        {
            Fields,
            Paging,
            OrderBy
        };
    }
}