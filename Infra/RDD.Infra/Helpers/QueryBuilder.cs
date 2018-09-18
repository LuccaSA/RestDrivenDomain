using NExtends.Expressions;
using RDD.Domain;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Models;
using RDD.Infra.Exceptions;
using RDD.Infra.Web.Models;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Infra.Helpers
{
    public class QueryBuilder<TEntity, TKey>
        where TEntity : IPrimaryKey<TKey>
    {
        private const int EF_EXPRESSION_TREE_MAX_DEPTH = 1000;

        public Expression<Func<TEntity, bool>> OrFactory<TProp>(Func<TProp, Expression<Func<TEntity, bool>>> filter, IList values)
        {
            if (values.Count > EF_EXPRESSION_TREE_MAX_DEPTH)
            {
                throw new QueryBuilderException(string.Empty, new ArgumentOutOfRangeException(nameof(values), $"OrFactory method invoked with {values.Count} values. Must be less than {EF_EXPRESSION_TREE_MAX_DEPTH} to be allowed."));
            }

            return values.OfType<TProp>().Select(val => filter(val)).OrAggregation();
        }

        public Expression<Func<TEntity, bool>> AndFactory<TProp>(Func<TProp, Expression<Func<TEntity, bool>>> filter, IList values)
        {
            if (values.Count > EF_EXPRESSION_TREE_MAX_DEPTH)
            {
                throw new QueryBuilderException(string.Empty, new ArgumentOutOfRangeException(nameof(values), $"AndFactory method invoked with {values.Count} values. Must be less than {EF_EXPRESSION_TREE_MAX_DEPTH} to be allowed."));
            }
            return values.OfType<TProp>().Select(val => filter(val)).AndAggregation();
        }

        public virtual Expression<Func<TEntity, bool>> Equals(IExpression field, IList values)
        {
            return BuildBinaryExpression(WebFilterOperand.Equals, field, values);
        }

        public virtual Expression<Func<TEntity, bool>> Equals(TKey key) => t => t.Id.Equals(key);

        public virtual Expression<Func<TEntity, bool>> NotEqual(IExpression field, IList values)
        {
            return AndFactory<object>(value => BuildBinaryExpression(WebFilterOperand.NotEqual, field, value), values);
        }

        public Expression<Func<TEntity, bool>> Until(IExpression field, IList values) => OrFactory<object>(value => Until(field, value), values);
        protected virtual Expression<Func<TEntity, bool>> Until(IExpression field, object value) => BuildBinaryExpression(WebFilterOperand.Until, field, value);

        public Expression<Func<TEntity, bool>> Since(IExpression field, IList values) => OrFactory<object>(value => Since(field, value), values);
        protected virtual Expression<Func<TEntity, bool>> Since(IExpression field, object value) => BuildBinaryExpression(WebFilterOperand.Since, field, value);

        public Expression<Func<TEntity, bool>> Anniversary(IExpression field, IList values) => OrFactory<object>(value => Anniversary(field, value), values);
        protected virtual Expression<Func<TEntity, bool>> Anniversary(IExpression field, object value) => BuildBinaryExpression(WebFilterOperand.Anniversary, field, value);

        public Expression<Func<TEntity, bool>> GreaterThan(IExpression field, IList values) => OrFactory<object>(value => GreaterThan(field, value), values);
        protected virtual Expression<Func<TEntity, bool>> GreaterThan(IExpression field, object value) => BuildBinaryExpression(WebFilterOperand.GreaterThan, field, value);

        public Expression<Func<TEntity, bool>> GreaterThanOrEqual(IExpression field, IList values) => OrFactory<object>(value => GreaterThanOrEqual(field, value), values);
        protected virtual Expression<Func<TEntity, bool>> GreaterThanOrEqual(IExpression field, object value) => BuildBinaryExpression(WebFilterOperand.GreaterThanOrEqual, field, value);

        public Expression<Func<TEntity, bool>> LessThan(IExpression field, IList values) => OrFactory<object>(value => LessThan(field, value), values);
        protected virtual Expression<Func<TEntity, bool>> LessThan(IExpression field, object value) => BuildBinaryExpression(WebFilterOperand.LessThan, field, value);

        public Expression<Func<TEntity, bool>> LessThanOrEqual(IExpression field, IList values) => OrFactory<object>(value => LessThanOrEqual(field, value), values);
        protected virtual Expression<Func<TEntity, bool>> LessThanOrEqual(IExpression field, object value) => BuildBinaryExpression(WebFilterOperand.LessThanOrEqual, field, value);

        public Expression<Func<TEntity, bool>> Between(IExpression field, IList values) => OrFactory<object>(value => Between(field, value), values);
        protected virtual Expression<Func<TEntity, bool>> Between(IExpression field, object value) => BuildBinaryExpression(WebFilterOperand.Between, field, value);

        public Expression<Func<TEntity, bool>> Starts(IExpression field, IList values) => OrFactory<string>(value => Starts(field, value), values);
        protected virtual Expression<Func<TEntity, bool>> Starts(IExpression field, string value)
        {
            var lambda = field.ToLambdaExpression();
            var parameter = lambda.Parameters[0];
            var comparisonMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
            var toLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });

            var startsWithExpression = Expression.Call(Expression.Call(lambda.Body, toLowerMethod), comparisonMethod, Expression.Constant(value.ToLower(), typeof(string)));

            return Expression.Lambda<Func<TEntity, bool>>(startsWithExpression, parameter);
        }

        public Expression<Func<TEntity, bool>> ContainsAll(IExpression field, IList values) => AndFactory<object>(value => BuildBinaryExpression(WebFilterOperand.ContainsAll, field, value), values);

        public Expression<Func<TEntity, bool>> Like(IExpression field, IList values) => OrFactory<object>(value => Like(field, value.ToString()), values);
        protected virtual Expression<Func<TEntity, bool>> Like(IExpression field, object value)
        {
            var lambda = field.ToLambdaExpression();
            var parameter = lambda.Parameters[0];
            var comparisonMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
            var toLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });
            var toStringMethod = typeof(object).GetMethod("ToString", new Type[] { });

            var containsExpression = Expression.Call(Expression.Call(Expression.Call(lambda.Body, toStringMethod), toLowerMethod), comparisonMethod, Expression.Constant(value.ToString().ToLower(), typeof(string)));
            return Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);
        }

        private Expression<Func<TEntity, bool>> BuildBinaryExpression(WebFilterOperand binaryOperator, IExpression field, object value)
        {
            var fieldLambda = field.ToLambdaExpression();
            var property = (fieldLambda.Body as MemberExpression)?.Member as PropertyInfo;
            var expression = BuildBinaryExpression(binaryOperator, fieldLambda.Body, value, property);
            
            var parameter = fieldLambda.Parameters[0];
            return Expression.Lambda<Func<TEntity, bool>>(expression, parameter);
        }

        private Expression BuildBinaryExpression(WebFilterOperand binaryOperator, Expression expressionLeft, object value, PropertyInfo property)
        {
            ConstantExpression expressionRight;
            switch (binaryOperator)
            {
                case WebFilterOperand.Until:
                case WebFilterOperand.Since:
                    var propertyReturnType = property.GetGetMethod().ReturnType;
                    if (propertyReturnType.IsGenericType)
                    {
                        propertyReturnType = propertyReturnType.GenericTypeArguments[0];
                    }
                    if (propertyReturnType != typeof(DateTime))
                    {
                        throw new QueryBuilderException($"Operator '{binaryOperator}' only allows date comparison");
                    }
                    expressionRight = Expression.Constant(value, expressionLeft.Type);
                    break;
                case WebFilterOperand.Between:
                    if (value != null && property == null)
                    {
                        throw new ArgumentNullException(nameof(property));
                    }
                    var period = (Period)value;
                    var expressionRightSince = (value == null) ? Expression.Constant(null) : Expression.Constant(period.Start, property.PropertyType);
                    var expressionRightUntil = (value == null) ? Expression.Constant(null) : Expression.Constant(period.End, property.PropertyType);
                    var sinceExpression = Expression.GreaterThanOrEqual(expressionLeft, expressionRightSince);
                    var untilExpression = Expression.LessThanOrEqual(expressionLeft, expressionRightUntil);
                    return Expression.AndAlso(sinceExpression, untilExpression);

                case WebFilterOperand.Anniversary:
                    if (property == null)
                    {
                        throw new ArgumentNullException(nameof(property));
                    }
                    var date = (DateTime?)value;
                    var day = date.HasValue ? Expression.Constant(date.Value.Day, typeof(int)) : Expression.Constant(null);
                    var month = date.HasValue ? Expression.Constant(date.Value.Month, typeof(int)) : Expression.Constant(null);
                    var dayExpression = property.PropertyType == typeof(DateTime?) ? Expression.Equal(day, Expression.Property(Expression.Property(expressionLeft, "Value"), "Day")) : Expression.Equal(day, Expression.Property(expressionLeft, "Day"));
                    var monthExpression = property.PropertyType == typeof(DateTime?) ? Expression.Equal(month, Expression.Property(Expression.Property(expressionLeft, "Value"), "Month")) : Expression.Equal(month, Expression.Property(expressionLeft, "Month"));
                    return Expression.AndAlso(dayExpression, monthExpression);
                case WebFilterOperand.Equals:
                    expressionRight = Expression.Constant(value);
                    break;
                default:
                    //précision du type nécessaire pour les nullables
                    expressionRight = Expression.Constant(value, expressionLeft.Type);
                    break;
            }

            switch (binaryOperator)
            {
                case WebFilterOperand.Equals: return Expression.Call(typeof(Enumerable), "Contains", new Type[] { expressionLeft.Type }, expressionRight, expressionLeft);
                case WebFilterOperand.NotEqual: return Expression.NotEqual(expressionLeft, expressionRight);
                case WebFilterOperand.GreaterThan: return Expression.GreaterThan(expressionLeft, expressionRight);
                case WebFilterOperand.LessThan: return Expression.LessThan(expressionLeft, expressionRight);

                case WebFilterOperand.Since:
                case WebFilterOperand.GreaterThanOrEqual:
                    if (value == null)
                    {
                        return Expression.Equal(expressionLeft, expressionRight);
                    }

                    return Expression.GreaterThanOrEqual(expressionLeft, expressionRight);

                case WebFilterOperand.Until:
                case WebFilterOperand.LessThanOrEqual:
                    if (value == null)
                    {
                        return Expression.Equal(expressionLeft, expressionRight);
                    }

                    return Expression.LessThanOrEqual(expressionLeft, expressionRight);

                case WebFilterOperand.ContainsAll: return Expression.Equal(expressionLeft, expressionRight);

                default:
                    throw new NotImplementedException($"L'expression binaire n'est pas gérée pour l'opérateur fourni: '{binaryOperator}'.");
            }
        }
    }
}