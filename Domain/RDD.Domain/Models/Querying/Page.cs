using RDD.Domain.Exceptions;
using System;

namespace RDD.Domain.Models.Querying
{
    public class Page
    {
        private const int MAX_LIMIT = int.MaxValue;
        public static readonly Page Unlimited = new Page(0, MAX_LIMIT);

        public int Offset { get; }
        public int Limit { get; }
        public int TotalCount { get; set; }

        private Page(int offset, int limit)
            : this(offset, limit, MAX_LIMIT) { }

        protected Page(int offset, int limit, int maxLimit)
        {
            var offsetConditions = offset >= 0;
            if (!offsetConditions)
            {
                throw new BadRequestException("Paging offset should be greater than 0", new ArgumentOutOfRangeException(nameof(offset)));
            }

            var limitConditions = limit >= 1 && limit <= maxLimit;
            if (!limitConditions)
            {
                throw new BadRequestException($"Paging limit should be between 1 and {maxLimit}", new ArgumentOutOfRangeException(nameof(limit)));
            }

            Offset = offset;
            Limit = limit;
        }
    }
}
