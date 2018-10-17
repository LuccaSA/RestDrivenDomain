﻿using NExtends.Expressions;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models;
using Rdd.Infra.Exceptions;
using Rdd.Infra.Web.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Rdd.Infra.Helpers
{
    public class WebFilterConverter<TEntity> : IWebFilterConverter<TEntity>
    {
        private const int EF_EXPRESSION_TREE_MAX_DEPTH = 1000;

        public Expression<Func<TEntity, bool>> ToExpression(IEnumerable<WebFilter<TEntity>> filters) => filters.Select(ToExpression).AndAggregation();

        public Expression<Func<TEntity, bool>> ToExpression(WebFilter<TEntity> filter)
        {
            switch (filter.Operand)
            {
                case WebFilterOperand.Equals: return Equals(filter.ExpressionChain, filter.Values);
                case WebFilterOperand.NotEqual: return NotEqual(filter.ExpressionChain, filter.Values);
                case WebFilterOperand.Starts: return Starts(filter.ExpressionChain, filter.Values);
                case WebFilterOperand.Like: return Like(filter.ExpressionChain, filter.Values);
                case WebFilterOperand.Between: return Between(filter.ExpressionChain, filter.Values);
                case WebFilterOperand.Since: return Since(filter.ExpressionChain, filter.Values);
                case WebFilterOperand.Until: return Until(filter.ExpressionChain, filter.Values);
                case WebFilterOperand.Anniversary: return Anniversary(filter.ExpressionChain, filter.Values);
                case WebFilterOperand.GreaterThan: return GreaterThan(filter.ExpressionChain, filter.Values);
                case WebFilterOperand.GreaterThanOrEqual: return GreaterThanOrEqual(filter.ExpressionChain, filter.Values);
                case WebFilterOperand.LessThan: return LessThan(filter.ExpressionChain, filter.Values);
                case WebFilterOperand.LessThanOrEqual: return LessThanOrEqual(filter.ExpressionChain, filter.Values);
                case WebFilterOperand.ContainsAll: return ContainsAll(filter.ExpressionChain, filter.Values);
                default: throw new NotImplementedException($"Unhandled operand : {filter.Operand}");
            }
        }

        protected Expression<Func<TEntity, bool>> OrFactory<TProp>(Func<TProp, Expression<Func<TEntity, bool>>> filter, IList values)
        {
            if (values.Count > EF_EXPRESSION_TREE_MAX_DEPTH)
            {
                throw new QueryBuilderException(string.Empty, new ArgumentOutOfRangeException(nameof(values), $"OrFactory method invoked with {values.Count} values. Must be less than {EF_EXPRESSION_TREE_MAX_DEPTH} to be allowed."));
            }

            return values.Cast<TProp>().Select(filter).OrAggregation();
        }

        protected Expression<Func<TEntity, bool>> AndFactory<TProp>(Func<TProp, Expression<Func<TEntity, bool>>> filter, IList values)
        {
            if (values.Count > EF_EXPRESSION_TREE_MAX_DEPTH)
            {
                throw new QueryBuilderException(string.Empty, new ArgumentOutOfRangeException(nameof(values), $"AndFactory method invoked with {values.Count} values. Must be less than {EF_EXPRESSION_TREE_MAX_DEPTH} to be allowed."));
            }
            return values.Cast<TProp>().Select(filter).AndAggregation();
        }

        public Expression<Func<TEntity, bool>> Equals(IExpression field, IList values) => BuildLambda(Contains, field, values);
        protected virtual Expression Contains(Expression leftExpression, IList values)
        {
            if (values.Count == 1)
            {
                return Expression.Equal(leftExpression, Expression.Constant(values[0]));
            }
            else
            {
                return Expression.Call(typeof(Enumerable), "Contains", new[] { leftExpression.Type }, Expression.Constant(values), leftExpression);
            }
        }

        public Expression<Func<TEntity, bool>> NotEqual(IExpression field, IList values) => BuildLambda(NotEqual, field, values);
        protected virtual Expression NotEqual(Expression leftExpression, IList values)
        {
            if (values.Count == 1)
            {
                return Expression.NotEqual(leftExpression, Expression.Constant(values[0]));
            }
            else
            {
                return Expression.Not(Contains(leftExpression, values));
            }
        }

        public Expression<Func<TEntity, bool>> ContainsAll(IExpression field, IList values) => AndFactory<object>(value => BuildLambda(Equal, field, value), values);
        protected virtual Expression Equal(Expression leftExpression, object value)
        {
            return Expression.Equal(leftExpression, Expression.Constant(value, leftExpression.Type));
        }

        public Expression<Func<TEntity, bool>> Since(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(GreaterThanOrEqual, field, value), values);
        public Expression<Func<TEntity, bool>> GreaterThanOrEqual(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(GreaterThanOrEqual, field, value), values);
        protected virtual Expression GreaterThanOrEqual(Expression leftExpression, object value)
        {
            if (value == null)
            {
                return Expression.Equal(leftExpression, Expression.Constant(value, leftExpression.Type));
            }
            else
            {
                return Expression.GreaterThanOrEqual(leftExpression, Expression.Constant(value, leftExpression.Type));
            }
        }

        public Expression<Func<TEntity, bool>> Until(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(LessThanOrEqual, field, value), values);
        public Expression<Func<TEntity, bool>> LessThanOrEqual(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(LessThanOrEqual, field, value), values);
        protected virtual Expression LessThanOrEqual(Expression leftExpression, object value)
        {
            if (value == null)
            {
                return Expression.Equal(leftExpression, Expression.Constant(value, leftExpression.Type));
            }
            else
            {
                return Expression.LessThanOrEqual(leftExpression, Expression.Constant(value, leftExpression.Type));
            }
        }

        public Expression<Func<TEntity, bool>> GreaterThan(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(GreaterThan, field, value), values);
        protected virtual Expression GreaterThan(Expression leftExpression, object value)
        {
            return Expression.GreaterThan(leftExpression, Expression.Constant(value, leftExpression.Type));
        }

        public Expression<Func<TEntity, bool>> LessThan(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(LessThan, field, value), values);
        protected virtual Expression LessThan(Expression leftExpression, object value)
        {
            return Expression.LessThan(leftExpression, Expression.Constant(value, leftExpression.Type));
        }

        public Expression<Func<TEntity, bool>> Anniversary(IExpression field, IList values) => OrFactory<DateTime?>(value => BuildLambda(Anniversary, field, value), values);
        protected virtual Expression Anniversary(Expression leftExpression, DateTime? value)
        {
            if (leftExpression.Type == typeof(DateTime?))
            {
                if (!value.HasValue)
                {
                    return Expression.Equal(leftExpression, Expression.Constant(null));
                }
                else
                {
                    var day = Expression.Constant(value.Value.Day, typeof(int));
                    var month = Expression.Constant(value.Value.Month, typeof(int));
                    var valueExpression = Expression.Property(leftExpression, "Value");
                    var dayExpression = Expression.Equal(day, Expression.Property(valueExpression, "Day"));
                    var monthExpression = Expression.Equal(month, Expression.Property(valueExpression, "Month"));
                    var anniversaryExpression = Expression.AndAlso(dayExpression, monthExpression);

                    var hasValueExpression = Expression.Property(leftExpression, "HasValue");
                    return Expression.AndAlso(hasValueExpression, anniversaryExpression);
                }
            }
            else
            {
                var date = (DateTime)value;
                var day = Expression.Constant(date.Day, typeof(int));
                var month = Expression.Constant(date.Month, typeof(int));
                var dayExpression = Expression.Equal(day, Expression.Property(leftExpression, "Day"));
                var monthExpression = Expression.Equal(month, Expression.Property(leftExpression, "Month"));
                return Expression.AndAlso(dayExpression, monthExpression);
            }
        }

        public Expression<Func<TEntity, bool>> Between(IExpression field, IList values) => OrFactory<Period>(value => BuildLambda(Between, field, value), values);
        protected virtual Expression Between(Expression leftExpression, Period value)
        {
            var expressionRightSince = Expression.Constant(value.Start, leftExpression.Type);
            var expressionRightUntil = Expression.Constant(value.End, leftExpression.Type);
            var sinceExpression = Expression.GreaterThanOrEqual(leftExpression, expressionRightSince);
            var untilExpression = Expression.LessThanOrEqual(leftExpression, expressionRightUntil);
            return Expression.AndAlso(sinceExpression, untilExpression);
        }

        public Expression<Func<TEntity, bool>> Starts(IExpression field, IList values) => OrFactory<string>(value => BuildLambda(Starts, field, value), values);
        protected virtual Expression Starts(Expression leftExpression, string value)
        {
            var startsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            var toLower = typeof(string).GetMethod("ToLower", new Type[] { });

            return Expression.Call(Expression.Call(leftExpression, toLower), startsWith, Expression.Constant(value.ToLower(), typeof(string)));
        }

        public Expression<Func<TEntity, bool>> Like(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(Like, field, value.ToString()), values);
        protected virtual Expression Like(Expression leftExpression, string value)
        {
            var contains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var toLower = typeof(string).GetMethod("ToLower", new Type[] { });
            var toString = typeof(object).GetMethod("ToString", new Type[] { });

            return Expression.Call(Expression.Call(Expression.Call(leftExpression, toString), toLower), contains, Expression.Constant(value.ToLower(), typeof(string)));
        }

        protected Expression<Func<TEntity, bool>> BuildLambda<TValue>(Func<Expression, TValue, Expression> builder, IExpression field, TValue value)
        {
            var fieldLambda = field.ToLambdaExpression();
            var expression = builder(fieldLambda.Body, value);
            var parameter = fieldLambda.Parameters[0];
            return Expression.Lambda<Func<TEntity, bool>>(expression, parameter);
        }
    }
}