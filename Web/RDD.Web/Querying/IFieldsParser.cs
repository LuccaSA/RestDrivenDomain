using Rdd.Domain.Helpers.Expressions;
using System;

namespace Rdd.Web.Querying
{
    public interface IFieldsParser
    {
        IExpressionTree<TEntity> GetDeFaultFields<TEntity>(bool isCollectionCall);
        IExpressionTree ParseDefaultFields(Type type);
        IExpressionTree<TEntity> Parse<TEntity>(string fields);
    }
}