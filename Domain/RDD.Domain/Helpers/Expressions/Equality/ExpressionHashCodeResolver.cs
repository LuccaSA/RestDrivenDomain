using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions.Equality
{
    class ExpressionHashCodeResolver : ExpressionVisitor
    {
        private int _runningTotal;

        public int GetHashCodeFor(Expression obj)
        {
            _runningTotal = 0;
            Visit(obj);
            return _runningTotal;
        }

        public override Expression Visit(Expression node)
        {
            if (null == node) return null;
            var constantValue = ConstantValue.New(node);
            if (constantValue != null)
            {
                var constant = Expression.Constant(constantValue.Value);
                _runningTotal += node.GetHashCodeFor(constant.NodeType, constant.Type);
                return base.Visit(constant);
            }

            _runningTotal += node.GetHashCodeFor(node.NodeType, node.Type);
            return base.Visit(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _runningTotal += node.GetHashCodeFor(node.Method, node.IsLifted, node.IsLiftedToNull);
            return base.VisitBinary(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _runningTotal += node.GetHashCodeFor(node.Value);
            return base.VisitConstant(node);
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            _runningTotal += node.GetHashCodeFor(node.AddMethod);
            return base.VisitElementInit(node);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            _runningTotal += node.GetHashCodeFor(node.Indexer);
            return base.VisitIndex(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            _runningTotal += node.GetHashCodeFor(node.Name, node.TailCall);
            return base.VisitLambda(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _runningTotal += node.GetHashCodeFor(node.Member);
            return base.VisitMember(node);
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            _runningTotal += node.GetHashCodeFor(node.BindingType, node.Member);
            return base.VisitMemberBinding(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _runningTotal += node.GetHashCodeFor(node.Method);
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            _runningTotal += node.GetHashCodeFor(node.Constructor, node.Members);
            return base.VisitNew(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _runningTotal += node.GetHashCodeFor(node.IsByRef);
            return base.VisitParameter(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            _runningTotal += node.GetHashCodeFor(node.Method, node.IsLifted, node.IsLiftedToNull);
            return base.VisitUnary(node);
        }
    }
}