using Rdd.Domain.Exceptions;
using System;
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
            if (!request.Query.TryGetValue(Reserved.Paging, out var pageValue) || StringValues.IsNullOrEmpty(pageValue))
            {
                return _rddOptions.Value.DefaultPage;
            }

            var input = pageValue.ToString();

            if (input == "1") //...&paging=1 <=> &paging=0,100
            {
                return _rddOptions.Value.DefaultPage;
            }
            else //...&paging=x,y
            {
                var elements = input.Split(',');

                if (elements.Length == 2)
                {
                    if (!Int32.TryParse(elements[0], out int offset))
                    {
                        throw new BadRequestException(String.Format("Offset value {0} not in correct format", elements[0]));
                    }

                    if (!Int32.TryParse(elements[1], out int limit))
                    {
                        throw new BadRequestException(String.Format("Limit value {0} not in correct format", elements[1]));
                    }

                    return new Page(offset, limit, _rddOptions.Value.PagingMaximumLimit);
                }
                else
                {
                    throw new BadRequestException(String.Format("{0} does not respect limit=start,count format", input));
                }
            }
        }
    }
}
