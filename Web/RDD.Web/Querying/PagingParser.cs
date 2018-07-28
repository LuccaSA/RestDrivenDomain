using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Querying
{
    public class PagingParser : IPagingParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<RddOptions> _rddOptions;
        public PagingParser(IHttpContextAccessor httpContextAccessor, IOptions<RddOptions> rddOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            this._rddOptions = rddOptions;
        }

        public virtual QueryPaging ParsePaging( )
        {
            var qp = new QueryPaging(_rddOptions.Value);

            if (!_httpContextAccessor.HttpContext.Request.Query.TryGetValue(QueryTokens.Paging, out StringValues pagingValues))
            {
                return qp;
            }

            if (pagingValues.Count > 1)
            {
                throw new BadRequestException("Does not respect \"limit=pageIndex,itemsPerPage\" format");
            }

            string paging = pagingValues.FirstOrDefault();
            if (String.IsNullOrEmpty(paging))
            {
                return qp;
            }

            if (paging == "1") //...&paging=1 <=> &paging=0,100
            {
                qp.PageOffset = 0;
                qp.ItemPerPage = _rddOptions.Value.DefaultItemsPerPage;
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

                    qp.PageOffset = pageOffset;
                    qp.ItemPerPage = itemPerPage;
                }
                else
                {
                    throw new BadRequestException($"{paging} does not respect limit=start,count format");
                }
            }

            return qp;
        }
    }
}