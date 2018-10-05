using RDD.Domain.Helpers.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Runtime.Serialization;

namespace RDD.Domain.Models.Querying
{
    public class StringConverter : IStringConverter
    {
        public List<T> ConvertValues<T>(IExpression expression, IEnumerable<string> values)
            => (List<T>)ConvertValues(expression, values);

        public IList ConvertValues(IExpression expression, IEnumerable<string> values)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var listConstructorParamType = typeof(List<>).MakeGenericType(expression.ResultType);

            var result = (IList)Activator.CreateInstance(listConstructorParamType);
            foreach (string value in values)
            {
                result.Add(ConvertTo(expression.ResultType, value));
            }
            return result;
        }

        public T ConvertTo<T>(string input) => (T)ConvertTo(typeof(T), input);

        public object ConvertTo(Type type, string input)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if ((nullableType != null || !type.IsValueType) && (string.IsNullOrEmpty(input) || input == "null"))
            {
                if (input == string.Empty)
                {
                    return string.Empty;
                }
                return null;
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

        private object InterpretString(string input, CultureInfo culture, Type type)
        {
            if (type == typeof(MailAddress))
            {
                return new MailAddress(input);
            }
            else if (type.IsEnum)
            {
                return Enum.Parse(type, input, true);
            }
            else if (type == typeof(TimeSpan))
            {
                return TimeSpan.Parse(input, culture);
            }
            else if (type == typeof(Guid))
            {
                return Guid.Parse(input);
            }
            else if (type == typeof(decimal))
            {
                return decimal.Parse(input, NumberStyles.AllowExponent | NumberStyles.Number, culture);
            }
            else if (type == typeof(Uri))
            {
                return new Uri(input);
            }
            else if (type == typeof(DateTime))
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
    }
}