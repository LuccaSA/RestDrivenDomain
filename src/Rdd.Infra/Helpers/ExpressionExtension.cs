﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rdd.Infra.Helpers
{
    public static class ExpressionExtension
    {
        public static Expression ExtractExpression<T>(this T value)
        {
            if (typeof(T) == typeof(object))
            {
                throw new NotSupportedException("Only use typed values");
            }

            if (value == null)
            {
                return Expression.Constant(null, typeof(T));
            }

            return ((Expression<Func<T>>)(() => value)).Body;
        }

        public static Expression ExtractTypedExpression(this object value, Type type)
            => (Expression)_extractMethodInfo.MakeGenericMethod(type).Invoke(null, new[] { value });

        private static readonly MethodInfo _extractMethodInfo = typeof(ExpressionExtension).GetMethod(nameof(ExtractExpression), BindingFlags.Public | BindingFlags.Static);
    }
}