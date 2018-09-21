using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using RDD.Domain.Helpers.Expressions;
using RDD.Web.Serialization;

namespace RDD.Web.Querying
{
    public static class PropertyTreeNodeExtension
    {
        public static PropertyTreeNode ParseFields(this HttpContext httpContext)
        {
            if (httpContext.Request.Query.TryGetValue(QueryTokens.Fields, out var fieldValues))
            {
                return PropertyTreeNode.ParseFields(fieldValues);
            }
            return null;
        }
    }
}
