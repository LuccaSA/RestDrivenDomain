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
        private readonly IOptions<PagingOptions> _rddOptions;

        public PagingParser(IHttpContextAccessor httpContextAccessor, IOptions<PagingOptions> rddOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _rddOptions = rddOptions;
        }

        public virtual QueryPaging ParsePaging( )
        {
            if (!_httpContextAccessor.HttpContext.Request.Query.TryGetValue(QueryTokens.Paging, out StringValues pagingValues))
            {
                return new QueryPaging(_rddOptions.Value);
            }

            if (pagingValues.Count > 1)
            {
                throw new BadRequestException("Does not respect \"limit=pageIndex,itemsPerPage\" format");
            }

            string paging = pagingValues.FirstOrDefault();
            if (String.IsNullOrWhiteSpace(paging))
            {
                return new QueryPaging(_rddOptions.Value);
            }

            if (paging == "1") //...&paging=1 <=> &paging=0,100
            {
                int pageOffset = 0;
                int itemPerPage = _rddOptions.Value.DefaultItemsPerPage;
                return new QueryPaging(_rddOptions.Value, pageOffset, itemPerPage);
            }

            //...&paging=x,y
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

                return new QueryPaging(_rddOptions.Value, pageOffset, itemPerPage);
            }

            throw new BadRequestException($"{paging} does not respect limit=start,count format");
        }
    }
}