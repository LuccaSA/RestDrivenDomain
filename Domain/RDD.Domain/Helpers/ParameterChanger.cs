using System.Linq.Expressions;

namespace RDD.Domain.Helpers
{
	public class ParameterChanger : ExpressionVisitor
	{
	    private ParameterExpression Parameter { get; }

		public ParameterChanger(ParameterExpression parameter)
		{
			Parameter = parameter;
		}

		protected override Expression VisitParameter(ParameterExpression node) => Parameter;
    }
}
