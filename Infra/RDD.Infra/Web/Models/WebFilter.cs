using RDD.Domain.Helpers.Expressions;
using System.Collections;
using System.Collections.Generic;

namespace RDD.Infra.Web.Models
{
    public class WebFilter<TEntity>
    {
        public IExpressionChain ExpressionChain { get; private set; }
        public WebFilterOperand Operand { get; private set; }
        public IList Values { get; private set; }

        public WebFilter(IExpressionChain expressionChain, WebFilterOperand operand, IList values)
        {
            ExpressionChain = expressionChain;
            Operand = operand;
            Values = values;
        }
    }

    public class WebFilter<TEntity, TProp> : WebFilter<TEntity>
    {
        public WebFilter(IExpressionChain expressionChain, WebFilterOperand operand, TProp value)
            : base(expressionChain, operand, new List<TProp> { value }) { }
    }
}