﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Rdd.Domain.Helpers.Expressions.Utils
{
    /// <summary>
    /// This visitor lets you chain two <see cref="LambdaExpression"/> in one.
    /// <example>
    /// <code>
    /// <para />ExpressionChainer.Chain(u => u.Manager, m => m.Name); // u => u.Manager.Name 
    /// <para />ExpressionChainer.Chain(u => u.Collaborators, m => m.Name); // u => u.Collaborators.Select(c => c.Name) 
    /// <para />ExpressionChainer.Chain(o => o.SuperUser(SuperUser), (BaseUser b) => b.Name); // u => u.SuperUser.Name 
    /// </code>
    /// </example>
    /// </summary>
    public class ExpressionChainer : ExpressionVisitor
    {
        private LambdaExpression _entryPoint;
        private LambdaExpression _outExpression;

        public static LambdaExpression Chain(LambdaExpression entryPoint, LambdaExpression outExpression)
        {
            if (outExpression == null)
            {
                return entryPoint;
            }
            if (entryPoint == null)
            {
                return outExpression;
            }

            if (typeof(IEnumerable<>).MakeGenericType(outExpression.Parameters[0].Type).IsAssignableFrom(entryPoint.Body.Type))
            {
                var type = entryPoint.Body.Type.GetGenericArguments().First();
                var select = Expression.Call(typeof(Enumerable), "Select", new[] { type, outExpression.ReturnType }, entryPoint.Body, outExpression);
                return Expression.Lambda(select, entryPoint.Parameters[0]);
            }

            return new ExpressionChainer().ChainAll(entryPoint, outExpression);
        }

        protected ExpressionChainer() { }

        protected LambdaExpression ChainAll(LambdaExpression entryPoint, LambdaExpression outExpression)
        {
            _entryPoint = entryPoint;
            _outExpression = outExpression;

            return Expression.Lambda(Visit(outExpression.Body), entryPoint.Parameters[0]);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == _outExpression.Parameters[0] && _outExpression.Parameters[0].Type.IsAssignableFrom(_entryPoint.Body.Type))
            {
                return _entryPoint.Body;
            }

            return base.VisitParameter(node);
        }
    }
}