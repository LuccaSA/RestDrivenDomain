using Rdd.Domain.Helpers.Expressions;
using System;
using Microsoft.AspNetCore.Http;

namespace Rdd.Web.Querying
{
    public interface IFieldsParser
    {
        IExpressionTree ParseDefaultFields(Type type);
        IExpressionTree<TEntity> Parse<TEntity>(HttpRequest fields, bool isCollectionCall);
    }
}