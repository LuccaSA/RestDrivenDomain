using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Reflection;

namespace Rdd.Domain.Models.Querying
{
    public interface IStringConverter
    {
        IFilterValue ConvertValues(Type type, params string[] values);
        IFilterValue ConvertValues<T>(params string[] values);
    }

    public interface IFilterValue
    {
        Expression Quote();
        Type Type { get; }
        int Count { get; }
        bool IsNull { get; }
    }

    public class FilterValue<T> : IFilterValue
    {
        public FilterValue(T value)
        {
            Value = value;
        }
        public T Value { get; }
        public Expression Quote() => this.ExtractFilterExpression(_thisType, _valueMember);
        private static Type _thisType = typeof(FilterValue<T>);
        private static MemberInfo _valueMember = typeof(FilterValue<T>).GetMember(nameof(Value)).First();

        public Type Type => typeof(T);
        public int Count => 1;
        public bool IsNull => Value == null;
    }

    public interface IFilterValueArray
    {
        IEnumerable<IFilterValue> EnumerateFilterValues { get; }
    }

    public class FilterValueArray<T> : IFilterValue, IFilterValueArray
    {
        public FilterValueArray(T[] value)
        {
            Value = value;
        }
        public T[] Value { get; }
        public Expression Quote() => this.ExtractFilterExpression(_thisType, _valueMember);
        private static Type _thisType = typeof(FilterValueArray<T>);
        private static MemberInfo _valueMember = typeof(FilterValueArray<T>).GetMember(nameof(Value)).First();

        public Type Type => typeof(T);
        public int Count => Value.Length;
        public bool IsNull => false;
        public IEnumerable<IFilterValue> EnumerateFilterValues => Value.Select(i => new FilterValue<T>(i));
    }

    public static class FilterValueHelper
    {
        public static Expression ExtractExpression<T>(this T value)
        {
            if (typeof(T) == typeof(object))
            {
                throw new NotSupportedException("Only use typed values");
            }
            return ((Expression<Func<T>>)(() => value)).Body;
        }

        public static Expression ExtractFilterExpression<T>(this FilterValue<T> filterValue, Type type, MemberInfo memberInfo)
        {
            return Expression.MakeMemberAccess(Expression.Constant(filterValue, type), memberInfo);
        }
        public static Expression ExtractFilterExpression<T>(this FilterValueArray<T> filterValue, Type type, MemberInfo memberInfo)
        {
            return Expression.MakeMemberAccess(Expression.Constant(filterValue, type), memberInfo);
        }

        public static FilterValue<T> AsFilterValue<T>(this T value) => new FilterValue<T>(value);
        public static FilterValueArray<T> AsFilterValueArray<T>(this T[] value) => new FilterValueArray<T>(value);

        public static IFilterValue Parse(this string[] values, Type targetType)
        {
            if (values.Length == 0)
            {
                throw new NotImplementedException();
            }
            if (values.Length == 1)
            {
                MethodInfo genericValue = _parseValueMethodInfo.MakeGenericMethod(targetType);
                return (IFilterValue)genericValue.Invoke(null, new object[] { values[0] });
            }
            MethodInfo genericValues = _parseValuesMethodInfo.MakeGenericMethod(targetType);
            return (IFilterValue)genericValues.Invoke(null, new object[] { values });
        }

        private static readonly MethodInfo _parseValueMethodInfo = typeof(FilterValueHelper).GetMethod(nameof(ParseValue), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _parseValuesMethodInfo = typeof(FilterValueHelper).GetMethod(nameof(ParseValues), BindingFlags.NonPublic | BindingFlags.Static);

        private static IFilterValue ParseValue<T>(string value)
        {
            return new FilterValue<T>((T)InterpretString(value, typeof(T)));
        }

        private static IFilterValue ParseValues<T>(string[] values)
        {
            return new FilterValueArray<T>(values.Select(i => (T)InterpretString(i, typeof(T))).ToArray());
        }

        private static object InterpretString(string input, Type type)
        {
            CultureInfo culture = GetInterpretationCulture(type);

            if (type == typeof(MailAddress))
            {
                return new MailAddress(input);
            }
            if (type.IsEnum)
            {
                return ((int)Enum.Parse(type, input, true));
            }
            if (type == typeof(TimeSpan))
            {
                return TimeSpan.Parse(input, culture);
            }
            if (type == typeof(Guid))
            {
                return Guid.Parse(input);
            }
            if (type == typeof(decimal))
            {
                return decimal.Parse(input, NumberStyles.AllowExponent | NumberStyles.Number, culture);
            }
            if (type == typeof(Uri))
            {
                return new Uri(input);
            }
            if (type == typeof(DateTime))
            {
                switch (input)
                {
                    case "today": return DateTime.Today;
                    case "now": return DateTime.Now;
                    case "tomorrow": return DateTime.Today.AddDays(1);
                    case "yesterday": return DateTime.Today.AddDays(-1);
                }
            }

            return Convert.ChangeType(input, type, culture);
        }

        private static CultureInfo GetInterpretationCulture(Type type)
        {
            if (type == typeof(double) || type == typeof(decimal) || type == typeof(DateTime))
            {
                return CultureInfo.InvariantCulture;
            }
            return CultureInfo.CurrentCulture;
        }

    }

}