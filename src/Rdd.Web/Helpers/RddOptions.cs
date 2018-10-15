using System;
using System.Collections.Generic;
using Rdd.Domain.Models.Querying;

namespace Rdd.Web.Helpers
{
    /// <summary>
    /// Global configuration, for properties used in the framework
    /// </summary>
    public class RddOptions
    {
        /// <summary>
        /// Default number of items per page
        /// </summary>
        public int PagingLimit { get; set; } = 10;

        public int PagingMaximumLimit { get; set; } = 1000;
      
        public void IgnoreFilters(params string[] filters)
        {
            foreach (var filter in filters)
            {
                IgnoredFilters.Add(filter);
            }
        }

        internal Page DefaultPage
        {
            get
            {
                if (_defaultWebPage != null)
                {
                    return _defaultWebPage;
                }
                _defaultWebPage = new Page(0, PagingLimit, PagingMaximumLimit);
                return _defaultWebPage;
            }
        }

        internal HashSet<string> IgnoredFilters { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private Page _defaultWebPage;
    }
}