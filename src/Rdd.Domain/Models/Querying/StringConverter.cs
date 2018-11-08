using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;

namespace Rdd.Domain.Models.Querying
{
    public class StringConverter : IStringConverter
    {
        public IFilterValue ConvertValues(Type type, params string[] values)
        {
            return values.Parse(type);
        }

        public IFilterValue ConvertValues<T>(params string[] values)
        {
            return values.Parse(typeof(T));
        }

        public FilterValue<T> ConvertTo<T>(string input) => (FilterValue<T>)ConvertTo(typeof(T), input);

        public IFilterValue ConvertTo(Type type, string input)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if ((nullableType != null || !type.IsValueType) && (string.IsNullOrEmpty(input) || input == "null"))
            {
                if (input == string.Empty)
                {
                    return string.Empty.AsFilterValue();
                }
                return ((string)null).AsFilterValue();
            }

            var correctedType = nullableType ?? type;
            var correctedInput = GetConvertableValue(input, correctedType);
            var culture = GetInterpretationCulture(correctedType);

            return InterpretString(correctedInput, culture, correctedType);
        }

        private string GetConvertableValue(string input, Type type)
        {
            if (type == typeof(double) || type == typeof(decimal))
            {
                return input.Replace(",", ".");
            }

            if (type == typeof(Guid))
            {
                var temp = input.Replace("-", "");
                var emptyGuid = new string('0', 32);
                return emptyGuid.Replace(emptyGuid.Substring(0, temp.Length), temp);
            }

            return input;
        }

        private CultureInfo GetInterpretationCulture(Type type)
        {
            if (type == typeof(double) || type == typeof(decimal) || type == typeof(DateTime))
            {
                return CultureInfo.InvariantCulture;
            }
            return CultureInfo.CurrentCulture;
        }

        private IFilterValue InterpretString(string input, CultureInfo culture, Type type)
        {
            if (type == typeof(MailAddress))
            {
                return new MailAddress(input).AsFilterValue();
            }
            else if (type.IsEnum)
            {
                return ((int)Enum.Parse(type, input, true)).AsFilterValue();
            }
            else if (type == typeof(TimeSpan))
            {
                return TimeSpan.Parse(input, culture).AsFilterValue();
            }
            else if (type == typeof(Guid))
            {
                return Guid.Parse(input).AsFilterValue();
            }
            else if (type == typeof(decimal))
            {
                return decimal.Parse(input, NumberStyles.AllowExponent | NumberStyles.Number, culture).AsFilterValue();
            }
            else if (type == typeof(Uri))
            {
                return new Uri(input).AsFilterValue();
            }
            else if (type == typeof(DateTime))
            {
                switch (input)
                {
                    case "today": return DateTime.Today.AsFilterValue();
                    case "now": return DateTime.Now.AsFilterValue();
                    case "tomorrow": return DateTime.Today.AddDays(1).AsFilterValue();
                    case "yesterday": return DateTime.Today.AddDays(-1).AsFilterValue();
                }
            }

            var filterType = typeof(FilterValue<>).MakeGenericType(type);
            var filter = (IFilterValue)Activator.CreateInstance(filterType, new object[] {Convert.ChangeType(input, type, culture)});
            return filter;
        }
    }
}