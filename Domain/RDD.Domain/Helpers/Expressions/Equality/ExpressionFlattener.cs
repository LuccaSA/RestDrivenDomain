using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions.Equality
{
    internal interface IExpressionFlattener
    {
        List<Expression> Flatten(Expression expression);
    }

    internal class ExpressionFlattener : ExpressionVisitor, IExpressionFlattener
    {
        private List<Expression> _result;

        public List<Expression> Flatten(Expression expression)
        {
            _result = new List<Expression>();
            Visit(expression);
            return _result;
        }

        public override Expression Visit(Expression node)
        {
            var constantValue = ConstantValue.New(node);
            if (constantValue != null)
            {
                _result.Add(Expression.Constant(constantValue.Value));
                return node;
            }
            else
            {
                _result.Add(node);
                return base.Visit(node);
            }
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            return node.Update(
                VisitAndConvert(node.NewExpression, nameof(VisitMemberInit)),
                Visit(new ReadOnlyCollection<MemberBinding>(node.Bindings.OrderBy(b => b.Member.Name).ToList()), VisitMemberBinding)
            );
        }
    }
}