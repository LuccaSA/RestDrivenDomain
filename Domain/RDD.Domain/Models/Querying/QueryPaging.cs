using RDD.Domain.Exceptions;

namespace RDD.Domain.Models.Querying
{
    public class QueryPaging
    {
        public QueryPaging(RddOptions options)
        {
            _itemPerPage = options?.DefaultItemsPerPage ?? 100;
            _maximumItemsPerPage = options?.MaximumItemsPerPage ?? 1000;
            if (_itemPerPage > _maximumItemsPerPage)
                throw new OutOfRangeException($"RddOptions ItemPerPage error");
        }

        private readonly int _maximumItemsPerPage;
        private int _itemPerPage;

        /// <summary>
        /// Requested page offset
        /// </summary>
        public int PageOffset { get; set; }

        /// <summary>
        /// Requested items per page
        /// </summary>
        public int ItemPerPage
        {
            get => _itemPerPage;
            set
            {
                if (value > _maximumItemsPerPage)
                    throw new OutOfRangeException($"Maximum ItemPerPage is {_maximumItemsPerPage}");
                _itemPerPage = value;
            }
        }
    }
}