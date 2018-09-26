using System;
using System.Collections.Generic;

namespace RDD.Web.Querying
{
    /// <summary>
    /// Holds the specific querey token.
    /// This class is registered as singleton
    /// </summary>
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

        private readonly HashSet<string> _ignoredFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public QueryTokens()
        {
            _ignoredFilters.Add(Fields);
            _ignoredFilters.Add(Paging);
            _ignoredFilters.Add(OrderBy);
        }

        public bool IsTokenReserved(string token)
        {
            return _ignoredFilters.Contains(token);
        }

        /// <summary>
        /// This allows to explicitly not take into account elements that are not supposed to become auto-generated query members
        /// </summary>
        public void IgnoreFilters(params string[] filters)
        {
            foreach (var filter in filters)
            {
                _ignoredFilters.Add(filter);
            }
        }
    }
}