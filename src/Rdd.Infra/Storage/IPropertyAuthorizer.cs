using Rdd.Domain.Helpers.Expressions;

namespace Rdd.Infra.Storage
{
    public interface IPropertyAuthorizer<TEntity>
    {
        IExpressionTree IncludeWhiteList { get; }

        bool IsVisible(IExpressionChain property);
    }
}