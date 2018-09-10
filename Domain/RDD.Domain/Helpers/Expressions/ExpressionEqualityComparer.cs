using System.Collections.Generic;

namespace RDD.Domain.Helpers.Expressions
{
    public class ExpressionEqualityComparer : IEqualityComparer<IExpression>
    {
        public bool Equals(IExpression x, IExpression y)
        {
            return (x == null && y == null) || (x != null && x.Equals(y));
        }

        public int GetHashCode(IExpression obj) => (obj.Name.GetHashCode() * 23) + (obj.ResultType.GetHashCode() * 17);
    }
}
