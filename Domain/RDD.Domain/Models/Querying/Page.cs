using RDD.Domain.Exceptions;

namespace RDD.Domain.Models.Querying
{
    public class Page
    {
        private const int MAX_LIMIT = int.MaxValue;
        public static Page Unlimited = new Page(0, MAX_LIMIT);

        public int Offset { get; }
        public int Limit { get; }
        public int TotalCount { get; set; }

        private Page(int offset, int limit)
            : this(offset, limit, MAX_LIMIT) { }

        protected Page(int offset, int limit, int maxLimit)
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
