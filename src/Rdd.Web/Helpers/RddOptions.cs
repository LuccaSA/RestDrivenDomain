﻿using Rdd.Domain.Models.Querying;

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

        private Page _defaultWebPage;
    }
}