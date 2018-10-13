using Rdd.Domain.Exceptions;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Rdd.Domain.Models.Querying;
using Rdd.Web.Helpers;

namespace Rdd.Web.Querying
{
    public class PagingParser
    {
        public Page Parse(Dictionary<string, string> parameters, IOptions<RddOptions> rddOptions)
        {
            if (parameters.ContainsKey(Reserved.paging.ToString()))
            {
                return Parse(parameters[Reserved.paging.ToString()], rddOptions);
            }
            return rddOptions.Value.DefaultPage;
        }

        protected Page Parse(string paging, IOptions<RddOptions> rddOptions)
        {
            if (paging == "1") //...&paging=1 <=> &paging=0,100
            {
                return rddOptions.Value.DefaultPage;
            }
            else //...&paging=x,y
            {
                var elements = paging.Split(',');

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

                    return new Page(offset, limit, rddOptions.Value.MaximumPaging);
                }
                else
                {
                    throw new BadRequestException(String.Format("{0} does not respect limit=start,count format", paging));
                }
            }
        }
    }
}
