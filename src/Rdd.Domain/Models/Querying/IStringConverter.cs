using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Rdd.Domain.Models.Querying
{
    public interface IStringConverter
    {
        List<T> ConvertValues<T>(IEnumerable<string> values);
        T ConvertTo<T>(string input);

        IList ConvertValues(Type type, IEnumerable<string> values);
        object ConvertTo(Type type, string input);
    }

    public static class ExpressionExtension
    {
        public static Expression ExtractExpression<T>(this T value)
        {
            if (typeof(T) == typeof(object))
            {
                throw new NotSupportedException("Only use typed values");
            }
            return ((Expression<Func<T>>)(() => value)).Body;
        }

        public static Expression ExtractTypedExpression(this object value, Type type)
        {
            var method = typeof(ExpressionExtension)
                .GetMethod(nameof(ExtractExpression),BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                throw new NotSupportedException();
            }
            var generic = method.MakeGenericMethod(type);
            return (Expression)generic.Invoke(null, new object[] { value });
        }
    }
}