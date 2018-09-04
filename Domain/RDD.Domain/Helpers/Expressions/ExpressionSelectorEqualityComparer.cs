using System.Collections.Generic;

namespace RDD.Domain.Helpers.Expressions
{
    public class ExpressionSelectorEqualityComparer : IEqualityComparer<IExpressionSelector>
    {
        public bool Equals(IExpressionSelector x, IExpressionSelector y)
        {
            return (x == null && y == null) || (x != null && x.Equals(y));
        }

        public int GetHashCode(IExpressionSelector obj) => (obj.Name.GetHashCode() * 23) + (obj.ResultType.GetHashCode() * 17);
    }
}
