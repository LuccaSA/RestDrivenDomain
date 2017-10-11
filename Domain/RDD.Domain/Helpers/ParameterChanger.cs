using System.Linq.Expressions;

namespace RDD.Domain.Helpers
{
    public class ParameterChanger : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ParameterChanger(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node) => _parameter;
    }
}
