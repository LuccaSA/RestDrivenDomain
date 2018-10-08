using Rdd.Domain.Helpers.Expressions;

namespace Rdd.Web.Querying
{
    public interface IFieldsParser
    {
        IExpressionTree<TEntity> GetDeFaultFields<TEntity>(bool isCollectionCall);
        IExpressionTree<TEntity> Parse<TEntity>(string fields);
    }
}