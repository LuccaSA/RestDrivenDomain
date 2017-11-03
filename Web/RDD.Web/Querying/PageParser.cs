using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Net;

namespace RDD.Web.Querying
{
    public class PageParser<TEntity>
        where TEntity : class, IEntityBase
    {
        public Page Parse(Dictionary<string, string> parameters)
        {
            if (parameters.ContainsKey(Reserved.paging.ToString()))
            {
                return Parse(parameters[Reserved.paging.ToString()]);
            }

            return Page.Default;
        }

        protected Page Parse(string paging)
        {
            if (paging == "1") //...&paging=1 <=> &paging=0,100
            {
                return Page.Default;
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
                        throw new HttpLikeException(HttpStatusCode.BadRequest, String.Format("Offset value {0} not in correct format", elements[0]));
                    }

                    if (!Int32.TryParse(elements[1], out limit))
                    {
                        throw new HttpLikeException(HttpStatusCode.BadRequest, String.Format("Limit value {0} not in correct format", elements[1]));
                    }

                    return new Page(offset, limit);
                }
                else
                {
                    throw new HttpLikeException(HttpStatusCode.BadRequest, String.Format("{0} does not respect limit=start,count format", paging));
                }
            }
        }
    }
}
