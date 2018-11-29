using NExtends.Expressions;
using NExtends.Primitives.Types;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Models;
using Rdd.Infra.Exceptions;
using Rdd.Infra.Web.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;

namespace Rdd.Infra.Helpers
{
    public class WebFilterConverter
    {
        protected const int EF_EXPRESSION_TREE_MAX_DEPTH = 1000;

        protected static readonly HashSet<Type> KnownTypesEvaluatedClientSideWithHashCode
            = new HashSet<Type> { typeof(MailAddress) };

        protected WebFilterConverter() { }
    }

    public class WebFilterConverter<TEntity> : WebFilterConverter, IWebFilterConverter<TEntity>
    {
        public Expression<Func<TEntity, bool>> ToExpression(IEnumerable<WebFilter<TEntity>> filters) => filters.Select(ToExpression).AndAggregation();

        public Expression<Func<TEntity, bool>> ToExpression(WebFilter<TEntity> filter)
        {
            switch (filter.Operand)
            {
                case WebFilterOperand.Equals: return Equals(filter.Expression, filter.Values);
                case WebFilterOperand.NotEqual: return NotEqual(filter.Expression, filter.Values);
                case WebFilterOperand.Starts: return Starts(filter.Expression, filter.Values);
                case WebFilterOperand.Like: return Like(filter.Expression, filter.Values);
                case WebFilterOperand.Between: return Between(filter.Expression, filter.Values);
                case WebFilterOperand.Since: return Since(filter.Expression, filter.Values);
                case WebFilterOperand.Until: return Until(filter.Expression, filter.Values);
                case WebFilterOperand.Anniversary: return Anniversary(filter.Expression, filter.Values);
                case WebFilterOperand.GreaterThan: return GreaterThan(filter.Expression, filter.Values);
                case WebFilterOperand.GreaterThanOrEqual: return GreaterThanOrEqual(filter.Expression, filter.Values);
                case WebFilterOperand.LessThan: return LessThan(filter.Expression, filter.Values);
                case WebFilterOperand.LessThanOrEqual: return LessThanOrEqual(filter.Expression, filter.Values);
                case WebFilterOperand.ContainsAll: return ContainsAll(filter.Expression, filter.Values);
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
            if (values.Count == 1 && !KnownTypesEvaluatedClientSideWithHashCode.Contains(leftExpression.Type))
            {
                return Expression.Equal(leftExpression, values[0].ExtractTypedExpression(leftExpression.Type));
            }
            else
            {
                return Expression.Call(typeof(Enumerable), "Contains", new[] { leftExpression.Type }, values.ExtractTypedExpression(typeof(IEnumerable<>).MakeGenericType(leftExpression.Type)), leftExpression);
            }
        }

        public Expression<Func<TEntity, bool>> NotEqual(IExpression field, IList values) => BuildLambda(NotEqual, field, values);
        protected virtual Expression NotEqual(Expression leftExpression, IList values)
        {
            if (values.Count == 1 && !KnownTypesEvaluatedClientSideWithHashCode.Contains(leftExpression.Type))
            {
                return Expression.NotEqual(leftExpression, values[0].ExtractTypedExpression(leftExpression.Type));
            }
            else
            {
                return Expression.Not(Contains(leftExpression, values));
            }
        }

        public Expression<Func<TEntity, bool>> ContainsAll(IExpression field, IList values) => AndFactory<object>(value => BuildLambda(Equal, field, value), values);
        protected virtual Expression Equal(Expression leftExpression, object value)
        {
            return Expression.Equal(leftExpression, value.ExtractTypedExpression(leftExpression.Type));
        }

        public Expression<Func<TEntity, bool>> Since(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(GreaterThanOrEqual, field, value), values);
        public Expression<Func<TEntity, bool>> GreaterThanOrEqual(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(GreaterThanOrEqual, field, value), values);
        protected virtual Expression GreaterThanOrEqual(Expression leftExpression, object value)
        {
            if (value == null)
            {
                return Expression.Equal(leftExpression, value.ExtractTypedExpression(leftExpression.Type));
            }
            else
            {
                return Expression.GreaterThanOrEqual(leftExpression, value.ExtractTypedExpression(leftExpression.Type));
            }
        }

        public Expression<Func<TEntity, bool>> Until(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(LessThanOrEqual, field, value), values);
        public Expression<Func<TEntity, bool>> LessThanOrEqual(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(LessThanOrEqual, field, value), values);
        protected virtual Expression LessThanOrEqual(Expression leftExpression, object value)
        {
            if (value == null)
            {
                return Expression.Equal(leftExpression, value.ExtractTypedExpression(leftExpression.Type));
            }
            else
            {
                return Expression.LessThanOrEqual(leftExpression, value.ExtractTypedExpression(leftExpression.Type));
            }
        }

        public Expression<Func<TEntity, bool>> GreaterThan(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(GreaterThan, field, value), values);
        protected virtual Expression GreaterThan(Expression leftExpression, object value)
        {
            return Expression.GreaterThan(leftExpression, value.ExtractTypedExpression(leftExpression.Type));
        }

        public Expression<Func<TEntity, bool>> LessThan(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(LessThan, field, value), values);
        protected virtual Expression LessThan(Expression leftExpression, object value)
        {
            return Expression.LessThan(leftExpression, value.ExtractTypedExpression(leftExpression.Type));
        }

        public Expression<Func<TEntity, bool>> Anniversary(IExpression field, IList values) => OrFactory<DateTime?>(value => BuildLambda(Anniversary, field, value), values);
        protected virtual Expression Anniversary(Expression leftExpression, DateTime? value)
        {
            if (leftExpression.Type == typeof(DateTime?))
            {
                if (!value.HasValue)
                {
                    return Expression.Equal(leftExpression, value.ExtractTypedExpression(typeof(DateTime?)));
                }
                else
                {
                    var day = value.Value.Day.ExtractExpression();
                    var month = value.Value.Month.ExtractExpression();
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
                var day = value.Value.Day.ExtractExpression();
                var month = value.Value.Month.ExtractExpression();
                var dayExpression = Expression.Equal(day, Expression.Property(leftExpression, "Day"));
                var monthExpression = Expression.Equal(month, Expression.Property(leftExpression, "Month"));
                return Expression.AndAlso(dayExpression, monthExpression);
            }
        }

        public Expression<Func<TEntity, bool>> Between(IExpression field, IList values) => OrFactory<Period>(value => BuildLambda(Between, field, value), values);
        protected virtual Expression Between(Expression leftExpression, Period value)
        {
            var expressionRightSince = value.Start.ExtractExpression();
            var expressionRightUntil = value.End.ExtractExpression();
            var sinceExpression = Expression.GreaterThanOrEqual(leftExpression, expressionRightSince);
            var untilExpression = Expression.LessThanOrEqual(leftExpression, expressionRightUntil);
            return Expression.AndAlso(sinceExpression, untilExpression);
        }

        public Expression<Func<TEntity, bool>> Starts(IExpression field, IList values) => OrFactory<string>(value => BuildLambda(Starts, field, value), values);
        protected virtual Expression Starts(Expression leftExpression, string value)
        {
            var startsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            var toLower = typeof(string).GetMethod("ToLower", new Type[] { });

            return Expression.Call(Expression.Call(leftExpression, toLower), startsWith, value.ToLower().ExtractExpression());
        }

        public Expression<Func<TEntity, bool>> Like(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(Like, field, value.ToString()), values);
        protected virtual Expression Like(Expression leftExpression, string value)
        {
            var contains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var toLower = typeof(string).GetMethod("ToLower", new Type[] { });
            var toString = typeof(object).GetMethod("ToString", new Type[] { });

            return Expression.Call(Expression.Call(Expression.Call(leftExpression, toString), toLower), contains, value.ToLower().ExtractExpression());
        }

        protected Expression<Func<TEntity, bool>> BuildLambda<TValue>(Func<Expression, TValue, Expression> builder, IExpression field, TValue value)
        {
            var fieldLambda = field.ToLambdaExpression();
            var isEnumerable = fieldLambda.Body.Type.IsEnumerableOrArray();

            var leftExpression = isEnumerable ? Expression.Parameter(field.ResultType) : fieldLambda.Body;            

            var expression = builder(leftExpression, value);
            if (isEnumerable)
            {
                var lambda = Expression.Lambda(expression, leftExpression as ParameterExpression);
                expression = Expression.Call(typeof(Enumerable), "Any", new[] { field.ResultType }, fieldLambda.Body, lambda);
            }

            var parameter = fieldLambda.Parameters[0];
            return Expression.Lambda<Func<TEntity, bool>>(expression, parameter);
        }
    }
}