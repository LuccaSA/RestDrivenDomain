using NExtends.Expressions;
using NExtends.Primitives.Types;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace RDD.Domain.Helpers
{
    public class PropertySelectorIncludablesExtractor : ExpressionVisitor
    {
        private readonly Func<PropertyInfo, bool> _IsIncludeCandidate;
        private bool DefaultIsIncludeCandidate(PropertyInfo property)
        {
            return property.PropertyType.IsSubclassOfInterface(typeof(IIncludable))
                        || (property.PropertyType.IsGenericType && property.PropertyType.GetGenericArguments()[0].IsSubclassOfInterface(typeof(IIncludable)));
        }

        public PropertySelectorIncludablesExtractor()
        {
            _IsIncludeCandidate = DefaultIsIncludeCandidate;
        }

        public PropertySelectorIncludablesExtractor(Func<PropertyInfo, bool> IsIncludeCandidate)
        {
            _IsIncludeCandidate = IsIncludeCandidate;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return VisitLambda((LambdaExpression)node);
        }

        protected Expression VisitLambda(LambdaExpression node)
        {
            Expression visited = Expression.Empty();

            if (node.Body.NodeType == ExpressionType.MemberAccess)
            {
                visited = VisitBody(node.Body as MemberExpression);
            }

            if (node.Body.NodeType == ExpressionType.Call)
            {
                visited = VisitMethodCall(node.Body as MethodCallExpression);
            }

            if (visited != null)
            {
                var param = node.Parameters[0];

                return Expression.Lambda(visited, param);
            }

            return null;
        }

        protected Expression VisitBody(MemberExpression node)
        {
            if (_IsIncludeCandidate((PropertyInfo)node.Member))
            {
                return node;
            }

            if (node.Expression is MemberExpression)
            {
                return VisitBody(node.Expression as MemberExpression);
            }

            return null;
        }

        protected Expression VisitMethodCall(MethodCallExpression node)
        {
            var lambda = (LambdaExpression)node.Arguments[1];

            var result = (LambdaExpression)VisitLambda(lambda);

            if (result != null)
            {
                var param = result.Parameters[0];
                var body = result.Body;
                var select = QueryableHelper.GetSelectMethod();
                select = select.MakeGenericMethod(param.Type, body.Type);

                return Expression.Call(null, select, node.Arguments[0], result);
            }

            var caller = VisitBody((MemberExpression)node.Arguments[0]);

            return caller;
        }
    }
}
