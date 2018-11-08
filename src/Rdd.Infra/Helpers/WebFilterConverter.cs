using NExtends.Expressions;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models;
using Rdd.Infra.Exceptions;
using Rdd.Infra.Web.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rdd.Domain.Models.Querying;

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
                case WebFilterOperand.Equals: return Equal(filter.ExpressionChain, filter.Values);
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

        protected Expression<Func<TEntity, bool>> OrFactory(Func<IFilterValue, Expression<Func<TEntity, bool>>> filter, IFilterValue value)
        {
            if (value.Count > EF_EXPRESSION_TREE_MAX_DEPTH)
            {
                throw new QueryBuilderException(string.Empty, new ArgumentOutOfRangeException(nameof(value), $"OrFactory method invoked with {value.Count} values. Must be less than {EF_EXPRESSION_TREE_MAX_DEPTH} to be allowed."));
            }
            if (value is IFilterValueArray values)
            {
                return values.EnumerateFilterValues.Select(filter).OrAggregation();
            }
            return filter(value);
        }

        protected Expression<Func<TEntity, bool>> AndFactory(Func<IFilterValue, Expression<Func<TEntity, bool>>> filter, IFilterValue value)
        {
            if (value.Count > EF_EXPRESSION_TREE_MAX_DEPTH)
            {
                throw new QueryBuilderException(string.Empty, new ArgumentOutOfRangeException(nameof(value), $"AndFactory method invoked with {value.Count} values. Must be less than {EF_EXPRESSION_TREE_MAX_DEPTH} to be allowed."));
            }
            if (value is IFilterValueArray values)
            {
                return values.EnumerateFilterValues.Select(filter).AndAggregation();
            }
            return filter(value);
        }

        public Expression<Func<TEntity, bool>> Equal(IExpression field, IFilterValue values) => BuildLambda(Contains, field, values);
        protected virtual Expression Contains(Expression leftExpression, IFilterValue values, Type propertyType)
        {
            if (values.Count == 1)
            {
                var v = values.Quote();
                return Expression.Equal(leftExpression, values.Quote());
            }
            else
            {
                return Expression.Call(typeof(Enumerable), "Contains", new[] { leftExpression.Type }, values.Quote(), leftExpression);
            }
        }

        public Expression<Func<TEntity, bool>> NotEqual(IExpression field, IFilterValue values) => BuildLambda(NotEqual, field, values);
        protected virtual Expression NotEqual(Expression leftExpression, IFilterValue values, Type propertyType)
        {
            if (values.Count == 1)
            {
                return Expression.NotEqual(leftExpression, values.Quote());
            }
            else
            {
                return Expression.Not(Contains(leftExpression, values, propertyType));
            }
        }

        public Expression<Func<TEntity, bool>> ContainsAll(IExpression field, IFilterValue values) => AndFactory(value => BuildLambda(Equal, field, value), values);
        protected virtual Expression Equal(Expression leftExpression, IFilterValue value, Type propertyType)
        {
            return Expression.Equal(leftExpression, value.Quote());
        }

        public Expression<Func<TEntity, bool>> Since(IExpression field, IFilterValue values) => OrFactory(value => BuildLambda(GreaterThanOrEqual, field, value), values);
        public Expression<Func<TEntity, bool>> GreaterThanOrEqual(IExpression field, IFilterValue values) => OrFactory(value => BuildLambda(GreaterThanOrEqual, field, value), values);
        protected virtual Expression GreaterThanOrEqual(Expression leftExpression, IFilterValue value, Type propertyType)
        {
            if (value.IsNull)
            {
                return Expression.Equal(leftExpression, Expression.Constant(value, leftExpression.Type));
            }
            else
            {
                return Expression.GreaterThanOrEqual(leftExpression, value.Quote());
            }
        }

        public Expression<Func<TEntity, bool>> Until(IExpression field, IFilterValue values) => OrFactory(value => BuildLambda(LessThanOrEqual, field, value), values);
        public Expression<Func<TEntity, bool>> LessThanOrEqual(IExpression field, IFilterValue values) => OrFactory(value => BuildLambda(LessThanOrEqual, field, value), values);
        protected virtual Expression LessThanOrEqual(Expression leftExpression, IFilterValue value, Type propertyType)
        {
            if (value.IsNull)
            {
                return Expression.Equal(leftExpression, Expression.Constant(value, leftExpression.Type));
            }
            else
            {
                return Expression.LessThanOrEqual(leftExpression, value.Quote());
            }
        }

        public Expression<Func<TEntity, bool>> GreaterThan(IExpression field, IFilterValue values) => OrFactory(value => BuildLambda(GreaterThan, field, value), values);
        protected virtual Expression GreaterThan(Expression leftExpression, IFilterValue value, Type propertyType)
        {
            return Expression.GreaterThan(leftExpression, value.Quote());
        }

        public Expression<Func<TEntity, bool>> LessThan(IExpression field, IFilterValue values) => OrFactory(value => BuildLambda(LessThan, field, value), values);
        protected virtual Expression LessThan(Expression leftExpression, IFilterValue value, Type propertyType)
        {
            return Expression.LessThan(leftExpression, value.Quote());
        }

        public Expression<Func<TEntity, bool>> Anniversary(IExpression field, IFilterValue values) => OrFactory(value => BuildLambda(Anniversary, field, value), values);

        protected virtual Expression Anniversary(Expression leftExpression, IFilterValue value, Type propertyType)
        {
            if (leftExpression.Type == typeof(DateTime?))
            {
                var dateValue = value as FilterValue<DateTime?>;
                if (dateValue == null)
                {
                    throw new NotImplementedException();
                }
                DateTime? date = dateValue.Value;
                if (!date.HasValue)
                {
                    return Expression.Equal(leftExpression, Expression.Constant(null));
                }
                else
                {
                    var day = date.Value.Day.ExtractExpression();
                    var month = date.Value.Month.ExtractExpression();
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
                var dateValue = value as FilterValue<DateTime>;
                if (dateValue == null)
                {
                    throw new NotImplementedException();
                }
                DateTime date = dateValue.Value;
                var day = date.Day.ExtractExpression();
                var month = date.Month.ExtractExpression();
                var dayExpression = Expression.Equal(day, Expression.Property(leftExpression, "Day"));
                var monthExpression = Expression.Equal(month, Expression.Property(leftExpression, "Month"));
                return Expression.AndAlso(dayExpression, monthExpression);
            }
        }

        public Expression<Func<TEntity, bool>> Between(IExpression field, IFilterValue values) => OrFactory(value => BuildLambda(Between, field, value), values);
        protected virtual Expression Between(Expression leftExpression, IFilterValue value, Type propertyType)
        {
            var periodValue = value as FilterValue<Period>;
            if (periodValue == null)
            {
                throw new NotImplementedException();
            }

            var expressionRightSince = periodValue.Value.Start.ExtractExpression();
            var expressionRightUntil = periodValue.Value.End.ExtractExpression();

            var sinceExpression = Expression.GreaterThanOrEqual(leftExpression, expressionRightSince);
            var untilExpression = Expression.LessThanOrEqual(leftExpression, expressionRightUntil);
            return Expression.AndAlso(sinceExpression, untilExpression);
        }

        public Expression<Func<TEntity, bool>> Starts(IExpression field, IFilterValue values) => OrFactory(value => BuildLambda(Starts, field, value), values);
        protected virtual Expression Starts(Expression leftExpression, IFilterValue value, Type propertyType)
        {
            var stringValue = value as FilterValue<string>;
            if (stringValue == null)
            {
                throw new NotImplementedException();
            }

            var startsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            var toLower = typeof(string).GetMethod("ToLower", new Type[] { });



            return Expression.Call(Expression.Call(leftExpression, toLower), startsWith, stringValue.Value.ToLower().ExtractExpression());
        }

        public Expression<Func<TEntity, bool>> Like(IExpression field, IFilterValue values) => OrFactory(value => BuildLambda(Like, field, value), values);
        protected virtual Expression Like(Expression leftExpression, IFilterValue value, Type propertyType)
        {
            var stringValue = value as FilterValue<string>;
            if (stringValue == null)
            {
                throw new NotImplementedException();
            }

            var contains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var toLower = typeof(string).GetMethod("ToLower", new Type[] { });
            var toString = typeof(object).GetMethod("ToString", new Type[] { });

            return Expression.Call(Expression.Call(Expression.Call(leftExpression, toString), toLower), contains, stringValue.Value.ToLower().ExtractExpression());
        }

        protected Expression<Func<TEntity, bool>> BuildLambda<TValue>(Func<Expression, TValue, Type, Expression> builder, IExpression field, TValue value)
        {
            var fieldLambda = field.ToLambdaExpression();
            var expression = builder(fieldLambda.Body, value, field.ResultType);
            var parameter = fieldLambda.Parameters[0];
            return Expression.Lambda<Func<TEntity, bool>>(expression, parameter);
        }
    }
}