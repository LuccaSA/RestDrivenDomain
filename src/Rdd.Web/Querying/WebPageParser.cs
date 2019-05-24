using Rdd.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Rdd.Domain.Models.Querying;
using Rdd.Web.Helpers;

namespace Rdd.Web.Querying
{
    public class PagingParser : IPagingParser
    {
        private readonly IOptions<RddOptions> _rddOptions;

        public PagingParser(IOptions<RddOptions> rddOptions)
        {
            _rddOptions = rddOptions;
        }

        public Page Parse(HttpRequest request)
        {
            if (!HttpMethods.IsGet(request.Method))
            {
                return Page.Unlimited;
            }

            if (!request.Query.TryGetValue(Reserved.Paging, out var pageValue) || StringValues.IsNullOrEmpty(pageValue))
            {
                return _rddOptions.Value.DefaultPage;
            }

            var input = pageValue.ToString();

            if (input == "1") //...&paging=1 <=> &paging=0,10
            {
                return _rddOptions.Value.DefaultPage;
            }
            else //...&paging=x,y
            {
                var elements = input.Split(',');

                if (elements.Length == 2)
                {
                    if (!int.TryParse(elements[0], out int offset))
                    {
                        throw new BadRequestException(string.Format("Offset value {0} not in correct format", elements[0]));
                    }

                    if (!int.TryParse(elements[1], out int limit))
                    {
                        throw new BadRequestException(string.Format("Limit value {0} not in correct format", elements[1]));
                    }

                    return new Page(offset, limit, _rddOptions.Value.PagingMaximumLimit);
                }
                else
                {
                    throw new BadRequestException(string.Format("{0} does not respect limit=start,count format", input));
                }
            }
        }
    }
}