using Microsoft.Extensions.Primitives;
using Rdd.Domain.Exceptions;
using System;
using System.Linq;

namespace Rdd.Web.Querying
{
    public class PagingParser : IPagingParser
    {
        public virtual WebPage Parse(string input)
        {
            var elements = new StringTokenizer(input ?? "", new[] { ',' }).ToList();
            if (elements.Count != 2 || !int.TryParse(elements[0], out var offset) || !int.TryParse(elements[1], out var limit))
            {
                throw new BadRequestException("Paging query parameter is invalid", new FormatException("Correct paging format is 'paging=offset,count'."));
            }

            return new WebPage(offset, limit);
        }
    }
}