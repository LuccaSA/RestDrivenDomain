using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions.Utils
{
    public class ExpressionChainExtractor : ExpressionVisitor
    {
        public Stack<IExpressionSelector> Selectors { get; set; }

        public ExpressionChainExtractor()
        {
            Selectors = new Stack<IExpressionSelector>();
        }

        public static IExpressionSelectorChain AsExpressionSelectorChain(LambdaExpression expression)
        {
            var extractor = new ExpressionChainExtractor();
            extractor.Visit(expression);
            return ApplyStack(extractor.Selectors);
        }

        private static ExpressionSelectorChain ApplyStack(Stack<IExpressionSelector> selectors)
        {
            if (selectors.Count == 0)
            {
                return null;
            }

            return new ExpressionSelectorChain
            {
                Current = selectors.Pop(),
                Next = ApplyStack(selectors)
            };
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var parameter = Expression.Parameter(node.Member.DeclaringType);
            var propertyExpression = Expression.PropertyOrField(parameter, node.Member.Name);
            var lambda = Expression.Lambda(propertyExpression, parameter);

            if (typeof(IEnumerable).IsAssignableFrom(node.Member.DeclaringType) && node.Member.DeclaringType != typeof(string))
            {
                Selectors.Push(new EnumerableMemberSelector { LambdaExpression = lambda });
            }
            else
            {
                Selectors.Push(new SimplePropertySelector { LambdaExpression = lambda });
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            var parameter = Expression.Parameter(node.Indexer.DeclaringType);
            var itemsExpression = Expression.MakeIndex(parameter, node.Indexer, node.Arguments);
            Selectors.Push(new ItemSelector { LambdaExpression = Expression.Lambda(itemsExpression, parameter) });

            return base.VisitIndex(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Enumerable))
            {
                Visit(new ReadOnlyCollection<Expression>(node.Arguments.Reverse().ToList()));
                return node;
            }
            else
            {
                return base.VisitMethodCall(node);
            }
        }
    }
}