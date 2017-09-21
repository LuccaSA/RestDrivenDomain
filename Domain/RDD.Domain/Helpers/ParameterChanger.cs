using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Helpers
{
	public class ParameterChanger : ExpressionVisitor
	{
		ParameterExpression Parameter { get; set; }

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
