using RDD.Domain.Exceptions;

namespace RDD.Domain.Models.Querying
{
    public class QueryPaging
    {
        public static readonly QueryPaging Default = new QueryPaging();

        private int _itemPerPage;

        public QueryPaging()
        {
            ItemPerPage = 10;
        }

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
                if (value > 1000)
                    throw new OutOfRangeException("Maximum ItemPerPage is 1000");
                _itemPerPage = value;
            }
        }
    }
}