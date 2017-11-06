using RDD.Domain.Exceptions;
using System.Net;

namespace RDD.Domain.Models.Querying
{
    public class Page
    {
        public const int MAX_LIMIT = 1000;
        public static Page Default => new Page(0, 10);

        public int Offset { get; }
        public int Limit { get; }
        public int TotalCount { get; set; }

        public Page(int offset, int limit)
            : this(offset, limit, MAX_LIMIT) { }

        public Page(int offset, int limit, int maxLimit)
        {
            var offsetConnditions = offset >= 0;
            if (!offsetConnditions)
            {
                throw new OutOfRangeException("Paging offset should be greater than 0");
            }

            var limitConditions = limit >= 1 && limit <= maxLimit;
            if (!limitConditions)
            {
                throw new OutOfRangeException($"Paging limit should be between 1 and {maxLimit}");
            }

            Offset = offset;
            Limit = limit;
        }
    }
}
