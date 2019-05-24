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
using System.Net.Mail;
using System.Reflection;

namespace Rdd.Infra.Helpers
{
    public class WebFilterConverter
    {
        protected const int EF_EXPRESSION_TREE_MAX_DEPTH = 1000;

        protected static readonly HashSet<Type> KnownTypesEvaluatedClientSideWithHashCode
            = new HashSet<Type> { typeof(MailAddress) };

        protected static readonly MethodInfo AnyMethod = typeof(Enumerable).GetMethods().First(mi => mi.Name == nameof(Enumerable.Any) && mi.GetParameters().Length == 2);

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
                    Expression day = value.Value.Day.ExtractExpression();
                    Expression month = value.Value.Month.ExtractExpression();
                    MemberExpression valueExpression = Expression.Property(leftExpression, "Value");
                    BinaryExpression dayExpression = Expression.Equal(day, Expression.Property(valueExpression, "Day"));
                    BinaryExpression monthExpression = Expression.Equal(month, Expression.Property(valueExpression, "Month"));
                    BinaryExpression anniversaryExpression = Expression.AndAlso(dayExpression, monthExpression);

                    MemberExpression hasValueExpression = Expression.Property(leftExpression, "HasValue");
                    return Expression.AndAlso(hasValueExpression, anniversaryExpression);
                }
            }
            else
            {
                Expression day = value.Value.Day.ExtractExpression();
                Expression month = value.Value.Month.ExtractExpression();
                BinaryExpression dayExpression = Expression.Equal(day, Expression.Property(leftExpression, "Day"));
                BinaryExpression monthExpression = Expression.Equal(month, Expression.Property(leftExpression, "Month"));
                return Expression.AndAlso(dayExpression, monthExpression);
            }
        }

        public Expression<Func<TEntity, bool>> Between(IExpression field, IList values) => OrFactory<Period>(value => BuildLambda(Between, field, value), values);
        protected virtual Expression Between(Expression leftExpression, Period value)
        {
            Expression expressionRightSince = value.Start.ExtractExpression();
            Expression expressionRightUntil = value.End.ExtractExpression();
            BinaryExpression sinceExpression = Expression.GreaterThanOrEqual(leftExpression, expressionRightSince);
            BinaryExpression untilExpression = Expression.LessThanOrEqual(leftExpression, expressionRightUntil);
            return Expression.AndAlso(sinceExpression, untilExpression);
        }

        public Expression<Func<TEntity, bool>> Starts(IExpression field, IList values) => OrFactory<string>(value => BuildLambda(Starts, field, value), values);
        protected virtual Expression Starts(Expression leftExpression, string value)
        {
            MethodInfo startsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            MethodInfo toLower = typeof(string).GetMethod("ToLower", new Type[] { });

            return Expression.Call(Expression.Call(leftExpression, toLower), startsWith, value.ToLower().ExtractExpression());
        }

        public Expression<Func<TEntity, bool>> Like(IExpression field, IList values) => OrFactory<object>(value => BuildLambda(Like, field, value.ToString()), values);
        protected virtual Expression Like(Expression leftExpression, string value)
        {
            MethodInfo contains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            MethodInfo toLower = typeof(string).GetMethod("ToLower", new Type[] { });
            MethodInfo toString = typeof(object).GetMethod("ToString", new Type[] { });

            return Expression.Call(Expression.Call(Expression.Call(leftExpression, toString), toLower), contains, value.ToLower().ExtractExpression());
        }

        protected Expression<Func<TEntity, bool>> BuildLambda<TValue>(Func<Expression, TValue, Expression> builder, IExpression field, TValue value)
        {
            var fieldLambda = field.ToLambdaExpression();
            Type fieldLambdaType = fieldLambda.Body.Type;

            Expression body;
            if (fieldLambdaType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(fieldLambdaType) && fieldLambdaType.GenericTypeArguments.Length > 0)
            {
                Type genericType = fieldLambdaType.GenericTypeArguments[0];
                ParameterExpression subParam = Expression.Parameter(genericType);
                LambdaExpression predicate = Expression.Lambda(typeof(Func<,>).MakeGenericType(genericType, typeof(bool)), builder(subParam, value), subParam);

                body = Expression.Call(AnyMethod.MakeGenericMethod(genericType), fieldLambda.Body, predicate);
            }
            else
            {
                body = builder(fieldLambda.Body, value);
            }

            return Expression.Lambda<Func<TEntity, bool>>(body, fieldLambda.Parameters[0]);
        }
    }
}