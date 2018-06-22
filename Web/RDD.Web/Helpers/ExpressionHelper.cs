using RDD.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Web.Helpers
{
    public class ExpressionHelper
    {
        // Utile pour les types créés à partir de Group By d'un autre type
        public static Expression<Func<TOut, bool>> Transfert<TIn, TOut>(Expression<Func<TIn, bool>> expression) => TransfertWithRename<TIn, TOut>(expression, new Dictionary<string, string>());

        /// <summary>
        /// Permet de transférer un délégué d'un enfant vers son parent étant connu le nom de la propriété qui permet de passer du parent à l'enfant
        /// </summary>
        /// <typeparam name="TChild">Le type enfant</typeparam>
        /// <typeparam name="TParent">Le type parent</typeparam>
        /// <param name="expression">L'expression (MemberExpression en général) sur l'enfant</param>
        /// <param name="childName">Le nom de la propriété permettant d'accéder à l'enfant depuis le parent</param>
        /// <returns></returns>
        public static LambdaExpression TransfertToParent<TChild, TParent>(LambdaExpression expression, string childName)
        {
            var param = Expression.Parameter(typeof(TParent), "p");
            var body = Expression.MakeMemberAccess(param, typeof(TParent).GetProperty(childName));

            Expression sub = Expression.PropertyOrField(body, ((PropertyInfo)(expression.Body as MemberExpression).Member).Name);

            return Expression.Lambda(sub, param);
        }
        public static Expression<Func<TOut, bool>> TransfertWithRename<TIn, TOut>(Expression<Func<TIn, bool>> expression, Dictionary<string, string> propertyRenaming)
        {
            var newParam = Expression.Parameter(typeof(TOut), "pOut");
            var visitor = new ParameterReplacementVisitor(expression.Parameters[0], newParam, propertyRenaming);
            var newParentBody = visitor.Visit(expression.Body);
            var exprOut = Expression.Lambda<Func<TOut, bool>>(newParentBody, newParam);
            return exprOut;
        }
        public static Expression<Func<TOut, object>> TransfertWithRename<TIn, TOut>(Expression<Func<TIn, object>> expression, Dictionary<string, string> propertyRenaming)
        {
            var newParam = Expression.Parameter(typeof(TOut), "pOut");
            var visitor = new ParameterReplacementVisitor(expression.Parameters[0], newParam, propertyRenaming);
            var newParentBody = visitor.Visit(expression.Body);
            var exprOut = Expression.Lambda<Func<TOut, object>>(newParentBody, newParam);
            return exprOut;
        }

        public class ParameterReplacementVisitor : ExpressionVisitor
        {
            private readonly Expression _oldParameter;
            private readonly Expression _newParameter;
            private readonly Dictionary<string, string> _propertyRenaming;

            public ParameterReplacementVisitor(ParameterExpression oldParameter, ParameterExpression newParameter, Dictionary<string, string> propertyRenaming)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
                _propertyRenaming = propertyRenaming;
            }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (_oldParameter == node)
                {
                    return _newParameter;
                }
                else
                {
                    return base.VisitParameter(node);
                }
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Member.DeclaringType == _oldParameter.Type)
                {
                    var propertyName = node.Member.Name;
                    if (_propertyRenaming.ContainsKey(propertyName))
                    {
                        propertyName = _propertyRenaming[propertyName];
                    }
                    var propertyInfo = _newParameter.Type.GetProperty(propertyName);
                    return Expression.MakeMemberAccess(_newParameter, propertyInfo);
                }
                else
                {
                    return base.VisitMember(node);
                }
            }
        }
        public static Expression BuildAny<TSource>(Expression accessor, Expression predicate) => BuildAny(typeof(TSource), accessor, predicate);

        public static Expression BuildAny(Type tSource, Expression accessor, Expression predicate) => BuildEnumerableMethod(tSource, accessor, predicate, "Any");

        public static Expression BuildWhere<TSource>(Expression accessor, Expression predicate) => BuildWhere(typeof(TSource), accessor, predicate);

        public static Expression BuildWhere(Type tSource, Expression accessor, Expression predicate) => BuildEnumerableMethod(tSource, accessor, predicate, "Where");

        private static Expression BuildEnumerableMethod(Type tSource, Expression accessor, Expression predicate, string method)
        {
            var overload = typeof(Enumerable).GetMethods()
                                      .Single(mi => mi.Name == method && mi.GetParameters().Length == 2).MakeGenericMethod(tSource);

            var call = Expression.Call(
                overload,
                accessor,
                predicate);

            return call;
        }

        public static bool HasSubexpressionsOfType<TTest>(Expression original)
        {
            var visitor = new HasTypeVisitor<TTest>();
            return visitor.VisitRoot(original);
        }

        private class HasTypeVisitor<TKeep> : ExpressionVisitor
        {
            private readonly Type _toKeep;
            private bool _HasType;
            public HasTypeVisitor()
            {
                _toKeep = typeof(TKeep);
            }
            public bool VisitRoot(Expression node)
            {
                var visited = Visit(node);
                return _HasType;
            }
            public override Expression Visit(Expression node)
            {
                if (_HasType) { return node; }
                if (node != null && node.Type == _toKeep)
                {
                    _HasType = true;
                    return node;
                }
                else
                {
                    return base.Visit(node);
                }
            }
        }

        public ICollection<Expression<Func<TEntity, object>>> Remove<TEntity>(IEnumerable<Expression<Func<TEntity, object>>> source, Expression<Func<TEntity, object>> candidate)
        {
            var currentIncludes = source;

            var result = new HashSet<Expression<Func<TEntity, object>>>();

            foreach (var exp in source)
            {
                if (!Equals(exp, candidate))
                {
                    result.Add(exp);
                }
            }

            return result;
        }

        public bool Equals<TEntity>(Expression<Func<TEntity, object>> exp1, Expression<Func<TEntity, object>> exp2) => GetPropertyFromPropertySelector(exp1) == GetPropertyFromPropertySelector(exp2);

        /// <summary>
        /// Récupère la propriété ciblée par une expression, notamment pour voir si 2 expressions sont égales = récupèrent la même propriété
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        private PropertyInfo GetPropertyFromPropertySelector<TEntity>(Expression<Func<TEntity, object>> propertySelector)
        {
            var propertyInfo = (propertySelector.Body as MemberExpression).Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
            }
            return propertyInfo;
        }

        public Expression NestedPropertyAccessor(ParameterExpression seed, string field)
        {
            PropertyInfo unusedFinalProperty;
            return NestedPropertyAccessor(seed, field, out unusedFinalProperty);
        }
        public Expression NestedPropertyAccessor(ParameterExpression seed, string field, out PropertyInfo property)
        {
            var type = seed.Type;
            return NestedPropertyAccessor(type, seed, field, out property);
        }
        public Expression NestedPropertyAccessor(Type type, ParameterExpression seed, string field, out PropertyInfo property) => NestedPropertyAccessor(type, seed, field.Split('.'), out property);

        private Expression NestedPropertyAccessor(Type type, ParameterExpression seed, string[] fields, out PropertyInfo property)
        {
            property = null;
            Expression body = seed;

            foreach (var member in fields)
            {
                property = type.GetProperties().FirstOrDefault(p => String.Equals(p.Name, member, StringComparison.CurrentCultureIgnoreCase));

                if (property == null)
                {
                    throw new BadRequestException(String.Format("Unknown property {0} on type {1}", member, type.Name));
                }

                if (!property.CanRead)
                {
                    throw new BadRequestException(String.Format("Property {0} of type {1} is set only", member, type.Name));
                }

                body = Expression.PropertyOrField(body, member);

                type = property.PropertyType;
            }

            return body;
        }
    }
}