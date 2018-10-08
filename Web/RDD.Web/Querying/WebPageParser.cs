using Rdd.Domain.Exceptions;
using System;
using System.Collections.Generic;

namespace Rdd.Web.Querying
{
    public class WebPageParser
    {
        public WebPage Parse(Dictionary<string, string> parameters)
        {
            if (parameters.ContainsKey(Reserved.paging.ToString()))
            {
                return Parse(parameters[Reserved.paging.ToString()]);
            }

            return WebPage.Default;
        }

        protected WebPage Parse(string paging)
        {
            if (paging == "1") //...&paging=1 <=> &paging=0,100
            {
                return WebPage.Default;
            }
            else //...&paging=x,y
            {
                var elements = paging.Split(',');

                if (elements.Length == 2)
                {
                    int offset;
                    int limit;

                    if (!Int32.TryParse(elements[0], out offset))
                    {
                        throw new BadRequestException(String.Format("Offset value {0} not in correct format", elements[0]));
                    }

                    if (!Int32.TryParse(elements[1], out limit))
                    {
                        throw new BadRequestException(String.Format("Limit value {0} not in correct format", elements[1]));
                    }

                    return new WebPage(offset, limit);
                }
                else
                {
                    throw new BadRequestException(String.Format("{0} does not respect limit=start,count format", paging));
                }
            }
        }
    }
}
