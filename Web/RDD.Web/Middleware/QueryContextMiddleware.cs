using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using RDD.Domain.Exceptions;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Middleware
{
    public class QueryContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly QueryRequest _queryRequest;

        public QueryContextMiddleware(RequestDelegate next, QueryRequest queryRequest)
        {
            _next = next;
            _queryRequest = queryRequest;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Query.TryGetValue(QueryTokens.Paging, out var pagingValues))
            {
                ParsePaging(_queryRequest, pagingValues);
            }

            await _next(context);
        }

        private void ParsePaging(QueryRequest queryRequest, StringValues pagingValues)
        {
            if (pagingValues.Count > 1)
            {
                throw new BadRequestException("Does not respect \"limit=pageIndex,itemsPerPage\" format");
            }

            string paging = pagingValues.FirstOrDefault();
            if (String.IsNullOrEmpty(paging))
            {
                return;
            }

            if (paging == "1") //...&paging=1 <=> &paging=0,100
            {
                queryRequest.PageOffset = 0;
                queryRequest.ItemPerPage = 100;
            }
            else //...&paging=x,y
            {
                var elements = paging.Split(',');

                if (elements.Length == 2)
                { 
                    if (!Int32.TryParse(elements[0], out var pageOffset))
                    {
                        throw new BadRequestException($"PageIndex value {elements[0]} not in correct format");
                    }

                    if (!Int32.TryParse(elements[1], out var itemPerPage))
                    {
                        throw new BadRequestException($"ItemsPerPage value {elements[1]} not in correct format");
                    }

                    queryRequest.PageOffset = pageOffset;
                    queryRequest.ItemPerPage = 100;
                }
                else
                {
                    throw new BadRequestException($"{paging} does not respect limit=start,count format");
                }
            }
        }
    }
}
