using System;
using RDD.Domain.Exceptions;

namespace RDD.Domain.Models.Querying
{
    public class QueryPaging
    {
        public QueryPaging(PagingOptions options)
        : this(options, 0, options.DefaultItemsPerPage)
        {
        }

        public QueryPaging(PagingOptions options, int offset, int itemsPerPage)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            PageOffset = offset;
            _itemPerPage = itemsPerPage;
            _maximumItemsPerPage = options.MaximumItemsPerPage;
            if (_itemPerPage > _maximumItemsPerPage)
            {
                throw new OutOfRangeException($"PagingOptions ItemPerPage error");
            }
        }

        private QueryPaging()
        {
            PageOffset = 0;
            _itemPerPage = MAX_LIMIT;
        }

        public static readonly QueryPaging Unlimited = new QueryPaging();

        private const int MAX_LIMIT = int.MaxValue;
        private readonly int _maximumItemsPerPage;
        private int _itemPerPage;

        /// <summary>
        /// Requested page offset
        /// </summary>
        public int PageOffset { get; }

        /// <summary>
        /// Requested items per page
        /// </summary>
        public int ItemPerPage
        {
            get => _itemPerPage;
            private set
            {
                if (value > _maximumItemsPerPage)
                {
                    throw new OutOfRangeException($"Maximum ItemPerPage is {_maximumItemsPerPage}");
                }
                _itemPerPage = value;
            }
        }
    }
}