using System.Linq.Expressions;

namespace RDD.Domain.Helpers
{
	public class ParameterChanger : ExpressionVisitor
	{
	    private ParameterExpression Parameter { get; set; }

		public ParameterChanger(ParameterExpression parameter)
		{
			Parameter = parameter;
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			return Parameter;
		}
	}
}
