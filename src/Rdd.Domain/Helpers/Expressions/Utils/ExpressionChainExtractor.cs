using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rdd.Domain.Helpers.Expressions.Utils
{
    /// <summary>
    /// This visitor lets you extract an <see cref="IExpressionChain"/> from a <see cref="LambdaExpression"/>
    /// <example>
    /// <code>
    /// <para />ExpressionChainExtractor.AsExpressionChain(u => u.Manager.Name); // u => u.Manager, m => m.Name);
    /// <para />ExpressionChainExtractor.AsExpressionChain(u => u.Collaborators.Select(c => c.Name)); // u => u.Collaborators, m => m.Name
    /// </code>
    /// </example>
    /// </summary>
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
            var parameter = Expression.Parameter(node.Expression.Type);
            var propertyExpression = Expression.PropertyOrField(parameter, node.Member.Name);
            //Ef needs the property coming from the declaring type
            if (propertyExpression.Member.ReflectedType != propertyExpression.Member.DeclaringType)
            {
                var property = propertyExpression.Member.DeclaringType.GetProperty(node.Member.Name, BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                propertyExpression = Expression.Property(parameter, property);
            }
            var lambda = Expression.Lambda(propertyExpression, parameter);

            if (typeof(IEnumerable).IsAssignableFrom(node.Expression.Type) && node.Expression.Type != typeof(string))
            {
                Expressions.Push(new EnumerablePropertyExpression(lambda));
            }
            else
            {
                Expressions.Push(new PropertyExpression(lambda));
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