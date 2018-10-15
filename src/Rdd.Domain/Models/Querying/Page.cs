using Rdd.Domain.Exceptions;
using System;

namespace Rdd.Domain.Models.Querying
{
    public class Page
    { 
        public static readonly Page Unlimited = new Page();

        public int Offset { get; }
        public int Limit { get; }
         
        private Page()
        {
            // Used only for Page.Unlimited
        }

        public Page(int offset, int limit, int maxLimit)
        {
            if (offset < 0)
            {
                throw new BadRequestException("Paging offset should be greater than 0", new ArgumentOutOfRangeException(nameof(offset)));
            }
            if (limit < 1 || limit > maxLimit)
            {
                throw new BadRequestException($"Paging limit should be between 1 and {maxLimit}", new ArgumentOutOfRangeException(nameof(limit)));
            }

            Offset = offset;
            Limit = limit;
        }
    }
}
