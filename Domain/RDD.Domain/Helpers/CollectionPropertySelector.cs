using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using NExtends.Primitives;
using RDD.Domain.Exceptions;

namespace RDD.Domain.Helpers
{
    public class CollectionPropertySelector<TEntity> : PropertySelector<TEntity>
    {
        public CollectionPropertySelector()
        {
            EntityType = typeof(ISelection<>).MakeGenericType(typeof(TEntity));
        }

        public override void Parse(string element, List<string> tail, int depth)
        {
            var specialMethods = new HashSet<string>
            {
                "sum",
                "min",
                "max"
            };

            if (specialMethods.Any(element.StartsWith))
            {
                string specialMethod = element.StartsWith("sum") ? "sum" : element.StartsWith("min") ? "min" : "max";
                GroupCollection matches = Regex.Match(element, string.Format("{0}\\(([a-zA-Z0-9_]*),?([a-zA-Z0-9_]*),?([a-zA-Z0-9_]*)\\)", specialMethod)).Groups;
                string propertyName = matches[1].Value;
                PropertyInfo property = GetEntityProperty(propertyName);
                DecimalRounding rouding = DecimalRounding.Parse(element);
                ParameterExpression param = Expression.Parameter(EntityType, "p".Repeat(depth));

                MethodCallExpression call = GetExpressionCall(specialMethod, rouding, param, property);

                LambdaExpression lambda = Expression.Lambda(call, param);

                PropertySelector child = NewFromType(EntityType);
                child.Lambda = lambda;
                child.Subject = propertyName;

                Children.Add(child);
            }
            else
            {
                base.Parse(element, tail, depth);
            }
        }

        private PropertyInfo GetEntityProperty(string propertyName)
        {
            PropertyInfo property = typeof(TEntity).GetProperties().FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.CurrentCultureIgnoreCase));

            if (property == null)
            {
                throw new BusinessException(string.Format("Unknown property {0} on type ISelection", propertyName));
            }

            return property;
        }

        private MethodCallExpression GetExpressionCall(string specialMethod, DecimalRounding rouding, ParameterExpression param, PropertyInfo property)
        {
            MethodInfo method = typeof(ISelection).GetMethod(specialMethod.ToFirstUpper(), new[] {typeof(PropertyInfo), typeof(DecimalRounding)});

            return Expression.Call(param, method, new Expression[] {Expression.Constant(property), Expression.Constant(rouding)});
        }
    }
}