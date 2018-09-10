using RDD.Domain.Helpers.Expressions;
using System.Collections;
using System.Collections.Generic;

namespace RDD.Infra.Web.Models
{
    public class WebFilter<TEntity>
    {
        public IExpressionChain Selector { get; private set; }
        public WebFilterOperand Operand { get; private set; }
        public IList Values { get; private set; }

        public WebFilter(IExpressionChain selector, WebFilterOperand operand, IList values)
        {
            Selector = selector;
            Operand = operand;
            Values = values;
        }
    }

    public class WebFilter<TEntity, TProp> : WebFilter<TEntity>
    {
        public WebFilter(IExpressionChain selector, WebFilterOperand operand, TProp value)
            : base(selector, operand, new List<TProp> { value }) { }
    }
}