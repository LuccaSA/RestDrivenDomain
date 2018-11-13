using Rdd.Domain.Helpers.Expressions;
using System.Collections;
using System.Collections.Generic;

namespace Rdd.Infra.Web.Models
{
    public class WebFilter<TEntity>
    {
        public IExpression Expression { get; private set; }
        public WebFilterOperand Operand { get; private set; }
        public IList Values { get; private set; }

        public WebFilter(IExpression expression, WebFilterOperand operand, IList values)
        {
            Expression = expression;
            Operand = operand;
            Values = values;
        }
    }
}