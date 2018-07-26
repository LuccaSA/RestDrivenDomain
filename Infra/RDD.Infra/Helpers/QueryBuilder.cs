using LinqKit;
using NExtends.Expressions;
using NExtends.Primitives.Types;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Infra.Exceptions;
using RDD.Infra.Web.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Infra.Helpers
{
    public class QueryBuilder<TEntity, TKey>
        where TEntity : IPrimaryKey<TKey>
    {
        private const int EF_EXPRESSION_TREE_MAX_DEPTH = 1000;

        public virtual IQueryable<TEntity> OrderBy(IQueryable<TEntity> entities, PropertySelector<TEntity> field, SortDirection direction, bool isFirst = true)
        {
            if (field == null) { return entities; }

            var type = typeof(TEntity);
            var parameter = Expression.Parameter(type, "entity");
            PropertyInfo property;
            var expression = NestedPropertyAccessor(parameter, field, out property);

            if (property.PropertyType == typeof(DateTime?))
            {
                return GetOrderyBy<DateTime?>(expression, parameter, entities, direction, isFirst);
            }
            if (property.PropertyType == typeof(DateTime))
            {
                return GetOrderyBy<DateTime>(expression, parameter, entities, direction, isFirst);
            }
            else if (property.PropertyType == typeof(int))
            {
                return GetOrderyBy<int>(expression, parameter, entities, direction, isFirst);
            }
            else if (property.PropertyType == typeof(decimal))
            {
                return GetOrderyBy<decimal>(expression, parameter, entities, direction, isFirst);
            }
            else if (property.PropertyType.IsEnum)
            {
                return GetOrderyBy<int>(Expression.Convert(expression, typeof(int)), parameter, entities, direction, isFirst);
            }
            else
            {
                return GetOrderyBy<object>(expression, parameter, entities, direction, isFirst);
            }
        }

        protected IQueryable<TEntity> GetOrderyBy<TPropertyType>(Expression property, ParameterExpression param, IQueryable<TEntity> entities, SortDirection direction, bool isFirst)
        {
            var mySortExpression = Expression.Lambda<Func<TEntity, TPropertyType>>(property, param);

            return GetOrderyBy(entities, mySortExpression, direction, isFirst);
        }
        protected IQueryable<TEntity> GetOrderyBy<TPropertyType>(IQueryable<TEntity> entities, Expression<Func<TEntity, TPropertyType>> mySortExpression, SortDirection direction, bool isFirst)
        {
            if (isFirst)
            {
                return (direction == SortDirection.Descending) ? entities.OrderByDescending(mySortExpression)
                    : entities.OrderBy(mySortExpression);
            }
            else
            {
                return (direction == SortDirection.Descending) ? ((IOrderedQueryable<TEntity>)entities).ThenByDescending(mySortExpression)
                    : ((IOrderedQueryable<TEntity>)entities).ThenBy(mySortExpression);
            }
        }

        public Expression<Func<TEntity, bool>> OrFactory<TProp>(Func<TProp, Expression<Func<TEntity, bool>>> filter, IList values)
        {
            if (values.Count > EF_EXPRESSION_TREE_MAX_DEPTH)
            {
                throw new QueryBuilderException(String.Empty, new ArgumentOutOfRangeException(nameof(values), $"OrFactory method invoked with {values.Count} values. Must be less than {EF_EXPRESSION_TREE_MAX_DEPTH} to be allowed."));
            }

            var expression = PredicateBuilder.False<TEntity>();
            foreach (TProp val in values)
            {
                expression = expression.Or(filter(val)).Expand();
            }
            return expression.Expand();
        }

        public Expression<Func<TEntity, bool>> AndFactory<TProp>(Func<TProp, Expression<Func<TEntity, bool>>> filter, IList values)
        {
            if (values.Count > EF_EXPRESSION_TREE_MAX_DEPTH)
            {
                throw new QueryBuilderException(String.Empty, new ArgumentOutOfRangeException(nameof(values), $"AndFactory method invoked with {values.Count} values. Must be less than {EF_EXPRESSION_TREE_MAX_DEPTH} to be allowed."));
            }

            var expression = PredicateBuilder.True<TEntity>();
            foreach (TProp val in values)
            {
                expression = expression.And(filter(val)).Expand();
            }
            return expression.Expand();
        }

        public virtual Expression<Func<TEntity, bool>> Equals(PropertySelector<TEntity> field, IList values)
        {
            return BuildBinaryExpression(WebFilterOperand.Equals, field, values);
        }

        public virtual Expression<Func<TEntity, bool>> Equals(TKey key)
        {
            return t => t.Id.Equals(key);
        }

        public virtual Expression<Func<TEntity, bool>> NotEqual(PropertySelector<TEntity> field, IList values)
        {
            return AndFactory<object>(value => BuildBinaryExpression(WebFilterOperand.NotEqual, field, value), values);
        }

        public Expression<Func<TEntity, bool>> Until(PropertySelector<TEntity> field, IList values)
        {
            return OrFactory<object>(value => Until(field, value), values);
        }
        protected virtual Expression<Func<TEntity, bool>> Until(PropertySelector<TEntity> field, object value)
        {
            return BuildBinaryExpression(WebFilterOperand.Until, field, value);
        }

        public Expression<Func<TEntity, bool>> Since(PropertySelector<TEntity> field, IList values)
        {
            return OrFactory<object>(value => Since(field, value), values);
        }
        protected virtual Expression<Func<TEntity, bool>> Since(PropertySelector<TEntity> field, object value)
        {
            return BuildBinaryExpression(WebFilterOperand.Since, field, value);
        }

        public Expression<Func<TEntity, bool>> Anniversary(PropertySelector<TEntity> field, IList values)
        {
            return OrFactory<object>(value => Anniversary(field, value), values);
        }
        protected virtual Expression<Func<TEntity, bool>> Anniversary(PropertySelector<TEntity> field, object value)
        {
            return BuildBinaryExpression(WebFilterOperand.Anniversary, field, value);
        }

        public Expression<Func<TEntity, bool>> GreaterThan(PropertySelector<TEntity> field, IList values)
        {
            return OrFactory<object>(value => GreaterThan(field, value), values);
        }
        protected virtual Expression<Func<TEntity, bool>> GreaterThan(PropertySelector<TEntity> field, object value)
        {
            return BuildBinaryExpression(WebFilterOperand.GreaterThan, field, value);
        }

        public Expression<Func<TEntity, bool>> GreaterThanOrEqual(PropertySelector<TEntity> field, IList values)
        {
            return OrFactory<object>(value => GreaterThanOrEqual(field, value), values);
        }
        protected virtual Expression<Func<TEntity, bool>> GreaterThanOrEqual(PropertySelector<TEntity> field, object value)
        {
            return BuildBinaryExpression(WebFilterOperand.GreaterThanOrEqual, field, value);
        }

        public Expression<Func<TEntity, bool>> LessThan(PropertySelector<TEntity> field, IList values)
        {
            return OrFactory<object>(value => LessThan(field, value), values);
        }
        protected virtual Expression<Func<TEntity, bool>> LessThan(PropertySelector<TEntity> field, object value)
        {
            return BuildBinaryExpression(WebFilterOperand.LessThan, field, value);
        }

        public Expression<Func<TEntity, bool>> LessThanOrEqual(PropertySelector<TEntity> field, IList values)
        {
            return OrFactory<object>(value => LessThanOrEqual(field, value), values);
        }
        protected virtual Expression<Func<TEntity, bool>> LessThanOrEqual(PropertySelector<TEntity> field, object value)
        {
            return BuildBinaryExpression(WebFilterOperand.LessThanOrEqual, field, value);
        }

        public Expression<Func<TEntity, bool>> Between(PropertySelector<TEntity> field, IList values)
        {
            return OrFactory<object>(value => Between(field, value), values);
        }
        protected virtual Expression<Func<TEntity, bool>> Between(PropertySelector<TEntity> field, object value)
        {
            return BuildBinaryExpression(WebFilterOperand.Between, field, value);
        }

        public Expression<Func<TEntity, bool>> Starts(PropertySelector<TEntity> field, IList values)
        {
            return OrFactory<string>(value => Starts(field, value), values);
        }
        protected virtual Expression<Func<TEntity, bool>> Starts(PropertySelector<TEntity> field, string value)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "entity");
            var expression = NestedPropertyAccessor(parameter, field);
            var comparisonMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
            var toLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });

            var startsWithExpression = Expression.Call(Expression.Call(expression, toLowerMethod), comparisonMethod, Expression.Constant(value.ToLower(), typeof(string)));

            return Expression.Lambda<Func<TEntity, bool>>(startsWithExpression, parameter);
        }

        public Expression<Func<TEntity, bool>> ContainsAll(PropertySelector<TEntity> field, IList values)
        {
            return AndFactory<object>(value => BuildBinaryExpression(WebFilterOperand.ContainsAll, field, value), values);
        }

        public Expression<Func<TEntity, bool>> Like(PropertySelector<TEntity> field, IList values)
        {
            return OrFactory<object>(value => Like(field, value.ToString()), values);
        }
        protected virtual Expression<Func<TEntity, bool>> Like(PropertySelector<TEntity> field, object value)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "entity");
            var expression = NestedPropertyAccessor(parameter, field);
            var comparisonMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
            var toLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });
            var toStringMethod = typeof(object).GetMethod("ToString", new Type[] { });

            var containsExpression = Expression.Call(Expression.Call(Expression.Call(expression, toStringMethod), toLowerMethod), comparisonMethod, Expression.Constant(value.ToString().ToLower(), typeof(string)));

            return Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);
        }

        private Expression<Func<TEntity, bool>> BuildBinaryExpression(WebFilterOperand binaryOperator, PropertySelector<TEntity> field, object value)
        {
            var type = typeof(TEntity);
            var parameter = Expression.Parameter(type, "entity");
            PropertyInfo property;
            var expression = BuildBinaryExpressionRecursive(binaryOperator, parameter, field, value, out property);

            // Limitation à certains types
            if (binaryOperator == WebFilterOperand.Until || binaryOperator == WebFilterOperand.Since)
            {
                var propertyReturnType = property.GetGetMethod().ReturnType;
                if (propertyReturnType.IsGenericType)
                {
                    propertyReturnType = propertyReturnType.GenericTypeArguments[0];
                }
                if (propertyReturnType != typeof(DateTime))
                {
                    throw new QueryBuilderException(String.Format("Operator '{2}' only allows dates to be compared, whereas property {0} is of type {1}.", field, property.GetType().Name, binaryOperator));
                }
            }

            return Expression.Lambda<Func<TEntity, bool>>(expression, parameter);
        }

        private Expression BuildBinaryExpressionRecursive(WebFilterOperand binaryOperator, ParameterExpression parameter, PropertySelector field, object value, out PropertyInfo property)
        {
            PropertySelector subField;
            Type collectionType;

            Expression expressionLeft = NestedPropertyAccessor(parameter.Type, parameter, field, out property);

            // Hack pour le Between qui n'est pas binaire, mais plus performant de le faire ici plutot que 2 parcours récursifs, puis un AND sur les expressions
            if (binaryOperator == WebFilterOperand.Between)
            {
                var period = (Period)value;
                ConstantExpression expressionRightSince = (value == null) ? Expression.Constant(null) : Expression.Constant(period.Start, property.PropertyType);
                ConstantExpression expressionRightUntil = (value == null) ? Expression.Constant(null) : Expression.Constant(period.End, property.PropertyType);
                var sinceExpression = Expression.GreaterThanOrEqual(expressionLeft, expressionRightSince);
                var untilExpression = Expression.LessThanOrEqual(expressionLeft, expressionRightUntil);
                return Expression.AndAlso(sinceExpression, untilExpression);
            }

            // Hack pour le Anniversary qui n'est pas binaire, mais plus simple de le faire ici plutot qu'ailleurs
            if (binaryOperator == WebFilterOperand.Anniversary)
            {
                var date = (DateTime?)value;
                ConstantExpression day = (date.HasValue) ? Expression.Constant(date.Value.Day, typeof(int)) : Expression.Constant(null);
                ConstantExpression month = (date.HasValue) ? Expression.Constant(date.Value.Month, typeof(int)) : Expression.Constant(null);
                var dayExpression = property.PropertyType == typeof(DateTime?) ? Expression.Equal(day, Expression.Property(Expression.Property(expressionLeft, "Value"), "Day")) : Expression.Equal(day, Expression.Property(expressionLeft, "Day"));
                var monthExpression = property.PropertyType == typeof(DateTime?) ? Expression.Equal(month, Expression.Property(Expression.Property(expressionLeft, "Value"), "Month")) : Expression.Equal(month, Expression.Property(expressionLeft, "Month"));
                return Expression.AndAlso(dayExpression, monthExpression);
            }

            ConstantExpression expressionRight;
            switch (binaryOperator)
            {
                case WebFilterOperand.Equals:
                    expressionRight = Expression.Constant(value);
                    break;
                default:
                    //précision du type nécessaie pour les nullables
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
                    throw new NotImplementedException(string.Format("L'expression binaire n'est pas gérée pour l'opérateur fourni: '{0}'.", binaryOperator));
            }
        }

        private Expression NestedPropertyAccessor(ParameterExpression seed, PropertySelector field)
        {
            PropertyInfo unusedFinalProperty;
            return NestedPropertyAccessor(seed, field, out unusedFinalProperty);
        }

        private Expression NestedPropertyAccessor(ParameterExpression seed, PropertySelector field, out PropertyInfo property)
        {
            var type = typeof(TEntity); //Ici on triche un peu en utilisant cette variable, car on est censé passer au moins 1 fois dans le foreach pour setter finalPropertyType proprement
            return NestedPropertyAccessor(type, seed, field, out property);
        }

        protected virtual Expression NestedPropertyAccessor(Type type, ParameterExpression seed, PropertySelector field, out PropertyInfo property)
        {
            property = null;
            Expression body = seed;
            var currentField = field;

            while (currentField != null)
            {
                // Include internal properties through BindingFlags
                property = type
                    .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .FirstOrDefault(p => p.Name.ToLower() == currentField.Name.ToLower());

                if (property == null)
                {
                    throw new QueryBuilderException($"Unknown property {currentField.Name} on type {type.Name}");
                }

                if (!property.CanRead)
                {
                    throw new QueryBuilderException($"Property {currentField.Name} of type {type.Name} is set only");
                }

                body = Expression.PropertyOrField(body, currentField.Name);

                type = property.PropertyType;
                currentField = currentField.Child;
            }

            return body;
        }
    }
}
