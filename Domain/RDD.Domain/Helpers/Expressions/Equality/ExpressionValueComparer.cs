using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Rdd.Domain.Helpers.Expressions.Equality
{
    //https://github.com/yuriy-nelipovich/LambdaCompare/blob/master/Neleus.LambdaCompare/Comparer.cs
    //https://github.com/yesmarket/yesmarket.Linq.Expressions/blob/master/yesmarket.Linq.Expressions/Support/ExpressionValueComparer.cs
    internal sealed class ExpressionValueComparer : ExpressionVisitor
    {
        private Queue<Expression> _tracked;
        private Expression _current;

        private bool _eq;

        public bool Compare(Expression x, Expression y)
        {
            _tracked = new Queue<Expression>(new ExpressionFlattener().Flatten(y));
            _current = null;
            _eq = true;

            Visit(x);

            return _eq;
        }

        public override Expression Visit(Expression node)
        {
            if (!_eq)
            {
                return node;
            }

            if (_tracked.Count == 0)
            {
                _eq = false;
                return node;
            }

            _current = _tracked.Dequeue();
            if (_current == null && node == null)
            {
                return base.Visit(node);
            }

            var testedNode = node;
            var constantValue = ConstantValue.New(node);
            if (constantValue != null)
            {
                testedNode = Expression.Constant(constantValue.Value);
            }

            if (_current == null || testedNode == null || _current.NodeType != testedNode.NodeType || !(_current.Type.IsAssignableFrom(testedNode.Type) || testedNode.Type.IsAssignableFrom(_current.Type)))
            {
                _eq = false;
                return node;
            }


            return base.Visit(testedNode);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var other = (BinaryExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Method, _ => _.IsLifted, _ => _.IsLiftedToNull);
            return _eq ? base.VisitBinary(node) : node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var other = (ConstantExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Value);
            return _eq ? base.VisitConstant(node) : node;
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            var other = (IndexExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Indexer);
            return _eq ? base.VisitIndex(node) : node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var other = (LambdaExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Name, _ => _.TailCall);
            return _eq ? base.VisitLambda(node) : node;
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            return _eq ? base.VisitListInit(node) : node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var other = (MemberExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Member);
            return _eq ? base.VisitMember(node) : node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            if (_eq)
            {
                return node.Update(
                    VisitAndConvert(node.NewExpression, nameof(VisitMemberInit)),
                    Visit(new ReadOnlyCollection<MemberBinding>(node.Bindings.OrderBy(b => b.Member.Name).ToList()), VisitMemberBinding)
                );
            }
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var other = (MethodCallExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Method);
            return _eq ? base.VisitMethodCall(node) : node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var other = (NewExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Constructor, _ => _.Members);
            return _eq ? base.VisitNew(node) : node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var other = (UnaryExpression)_current;
            _eq &= node.IsEqualTo(other, _ => _.Method, _ => _.IsLifted, _ => _.IsLiftedToNull);
            return _eq ? base.VisitUnary(node) : node;
        }
    }
}