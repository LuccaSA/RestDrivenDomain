using Rdd.Domain.Helpers.Expressions;
using System;
using Microsoft.AspNetCore.Http;

namespace Rdd.Web.Querying
{
    public interface IFieldsParser
    {
        IExpressionTree ParseDefaultFields(Type type);
    }

    public interface IFieldsParser<TEntity>
    {
        IExpressionTree<TEntity> Parse(HttpRequest request, bool isCollectionCall);
    }
}