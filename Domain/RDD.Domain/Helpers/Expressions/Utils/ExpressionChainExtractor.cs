using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Domain.Helpers.Expressions.Utils
{
    public class ExpressionChainExtractor : ExpressionVisitor
    {
        public Stack<IExpression> Expressions { get; set; }

        public ExpressionChainExtractor()
        {
            Expressions = new Stack<IExpression>();
        }

        public static IExpressionChain AsExpressionChain(LambdaExpression expression)
            => ApplyStack(GetExpressions(expression));

        public static IExpressionChain<TClass> AsExpressionChain<TClass, TProp>(Expression<Func<TClass, TProp>> expression)
            => ApplyStack<TClass>(GetExpressions(expression));

        private static Stack<IExpression> GetExpressions(LambdaExpression expression)
        {
            var extractor = new ExpressionChainExtractor();
            extractor.Visit(expression);
            return extractor.Expressions;
        }

        private static IExpressionChain<TClass> ApplyStack<TClass>(Stack<IExpression> expressions)
        {
            if (expressions.Count == 0)
            {
                return null;
            }

            return new ExpressionChain<TClass>
            {
                Current = expressions.Pop(),
                Next = ApplyStack(expressions)
            };
        }

        private static IExpressionChain ApplyStack(Stack<IExpression> expressions)
        {
            if (expressions.Count == 0)
            {
                return null;
            }

            return new ExpressionChain
            {
                Current = expressions.Pop(),
                Next = ApplyStack(expressions)
            };
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var parameter = Expression.Parameter(node.Member.DeclaringType);
            var propertyExpression = Expression.PropertyOrField(parameter, node.Member.Name);
            var lambda = Expression.Lambda(propertyExpression, parameter);

            if (typeof(IEnumerable).IsAssignableFrom(node.Member.DeclaringType) && node.Member.DeclaringType != typeof(string))
            {
                Expressions.Push(new EnumerablePropertyExpression { LambdaExpression = lambda });
            }
            else
            {
                Expressions.Push(new PropertyExpression { LambdaExpression = lambda });
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            var parameter = Expression.Parameter(node.Indexer.DeclaringType);
            var itemsExpression = Expression.MakeIndex(parameter, node.Indexer, node.Arguments);
            Expressions.Push(new ItemExpression { LambdaExpression = Expression.Lambda(itemsExpression, parameter) });

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