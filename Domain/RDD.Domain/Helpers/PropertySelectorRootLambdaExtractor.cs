using System;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers
{
    /// <summary>
    /// Ce visiteur permet de renvoyer le permier MemberAccess d'une LambdaExpression
    /// d => d.Users.Select(u => u.Name) va renvoyer d => d.Users
    /// </summary>
    public class PropertySelectorRootLambdaExtractor : ExpressionVisitor
    {
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Expression body = null;

            switch (node.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    {
                        body = VisitMember(node.Body as MemberExpression);
                        break;
                    }

                case ExpressionType.Call:
                    {
                        body = VisitMethodCall(node.Body as MethodCallExpression);
                        break;
                    }

                case ExpressionType.Convert:
                    {
                        body = VisitMember((node.Body as UnaryExpression).Operand as MemberExpression);
                        break;
                    }
            }

            return Expression.Lambda(body, node.Parameters[0]);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression is MemberExpression)
            {
                return VisitMember(node.Expression as MemberExpression);
            }

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var caller = node.Arguments[0];

            if (caller is MemberExpression)
            {
                return VisitMember(caller as MemberExpression);
            }

            if (caller is MethodCallExpression) //2 Select imbriqués par ex
            {
                return VisitMethodCall(caller as MethodCallExpression);
            }

            throw new NotImplementedException();
        }
    }
}
