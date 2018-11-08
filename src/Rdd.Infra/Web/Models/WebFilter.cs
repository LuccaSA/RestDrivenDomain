using System;
using Rdd.Domain.Helpers.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rdd.Domain.Models.Querying;

namespace Rdd.Infra.Web.Models
{
    public class WebFilter<TEntity>
    {
        public IExpressionChain ExpressionChain { get; private set; }
        public WebFilterOperand Operand { get; private set; }
        public IFilterValue Values { get; private set; }

        public WebFilter(IExpressionChain expressionChain, WebFilterOperand operand, IFilterValue values)
        {
            ExpressionChain = expressionChain;
            Operand = operand;
            Values = values;
        }
    }
}