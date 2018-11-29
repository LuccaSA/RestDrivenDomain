using Rdd.Domain.Helpers.Expressions;
using System.Linq;

namespace Rdd.Infra.Storage
{
    public class PropertyAuthorizer<TEntity> : IPropertyAuthorizer<TEntity>
    {
        public virtual IExpressionTree IncludeWhiteList { get; }

        public PropertyAuthorizer() : this(null) { }
        public PropertyAuthorizer(IExpressionTree whiteList)
        {
            IncludeWhiteList = whiteList;
        }

        public virtual bool IsVisible(IExpressionChain property) => IsVisible(property, IncludeWhiteList);

        protected virtual bool IsVisible(IExpressionChain property, IExpressionTree tree)
        {
            //leaves (actual properties) are visible, if base entity is visible
            if (property?.Next == null)
            {
                return true;
            }

            if (tree == null)
            {
                return false;
            }

            var subTree = tree.Children.FirstOrDefault(c => c.Node.Equals(property.Current));

            //property is not includable => not filterable either
            return subTree != null && IsVisible(property.Next, subTree);
        }
    }
}