using RDD.Domain.Exceptions;

namespace RDD.Domain.Models.Querying
{
    /// <summary>
    /// Holds the parsed query parameters
    /// </summary>
    public class QueryRequest
    {
        private int _itemPerPage;

        public QueryRequest()
        {
            ItemPerPage = 10;
            NeedEnumeration = true;
            CheckRights = true;
            WithWarnings = true;
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


        /// <summary>
        /// Est-ce qu'on a besoin du Count
        /// </summary>
        public bool NeedCount { get; set; }

        /// <summary>
        /// Est-ce qu'on a besoin d'énumérer la query
        /// </summary>
        public bool NeedEnumeration { get; set; }

        /// <summary>
        /// Should we FilterRights on GET request, or CheckRightForCreate on POST
        /// </summary>
        public bool CheckRights { get; set; }

        public bool WithWarnings { get; set; }
    }
}