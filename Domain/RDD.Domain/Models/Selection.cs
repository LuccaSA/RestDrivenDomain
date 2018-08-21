using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RDD.Domain.Models
{
    public class Selection<TEntity> : ISelection<TEntity>
        where TEntity : class
    {
        public IEnumerable<TEntity> Items { get; }
        public int Count { get; }

        public Selection(IEnumerable<TEntity> items, int count)
        {
            Items = items;
            Count = count;
        }

        public object Sum(PropertyInfo property, DecimalRounding rounding)
        {
            if (!Items.Any())
            {
                return 0;
            }

            if (property.PropertyType == typeof(int))
            {
                return Items.Sum(i => (int)property.GetValue(i));
            }
            if (property.PropertyType == typeof(int?))
            {
                return Items.Sum(i => (int?)property.GetValue(i) ?? 0);
            }
            if (property.PropertyType == typeof(double))
            {
                Func<double, double> roundFunction = rounding.GetDoubleRoundingFunction();
                double sum = Items.Sum(i => (double)property.GetValue(i));
                return roundFunction(sum);
            }
            if (property.PropertyType == typeof(double?))
            {
                Func<double, double> roundFunction = rounding.GetDoubleRoundingFunction();
                double sum = Items.Sum(i => (double?)property.GetValue(i) ?? 0);
                return roundFunction(sum);
            }
            if (property.PropertyType == typeof(decimal))
            {
                Func<decimal, decimal> roundFunction = rounding.GetDecimalRoundingFunction();
                decimal sum = Items.Sum(i => (decimal)property.GetValue(i));
                return roundFunction(sum);
            }
            if (property.PropertyType == typeof(decimal?))
            {
                Func<decimal, decimal> roundFunction = rounding.GetDecimalRoundingFunction();
                decimal sum = Items.Sum(i => (decimal?)property.GetValue(i) ?? 0);
                return roundFunction(sum);
            }
            if (property.PropertyType == typeof(TimeSpan))
            {
                return new TimeSpan(Items.Sum(i => ((TimeSpan)property.GetValue(i)).Ticks));
            }
            throw new NotImplementedException(String.Format("Unhandled type {0}", property.PropertyType.Name));
        }

        public object Min(PropertyInfo property, DecimalRounding rounding)
        {
            if (!Items.Any())
            {
                return 0;
            }

            if (property.PropertyType == typeof(int))
            {
                return Items.Min(i => (int)property.GetValue(i));
            }
            if (property.PropertyType == typeof(int?))
            {
                return Items.Min(i => (int?)property.GetValue(i) ?? 0);
            }
            if (property.PropertyType == typeof(double))
            {
                Func<double, double> roundFunction = rounding.GetDoubleRoundingFunction();
                double sum = Items.Min(i => (double)property.GetValue(i));
                return roundFunction(sum);
            }
            if (property.PropertyType == typeof(double?))
            {
                Func<double, double> roundFunction = rounding.GetDoubleRoundingFunction();
                double sum = Items.Min(i => (double?)property.GetValue(i) ?? 0);
                return roundFunction(sum);
            }
            if (property.PropertyType == typeof(decimal))
            {
                Func<decimal, decimal> roundFunction = rounding.GetDecimalRoundingFunction();
                decimal sum = Items.Min(i => (decimal)property.GetValue(i));
                return roundFunction(sum);
            }
            if (property.PropertyType == typeof(decimal?))
            {
                Func<decimal, decimal> roundFunction = rounding.GetDecimalRoundingFunction();
                decimal sum = Items.Min(i => (decimal?)property.GetValue(i) ?? 0);
                return roundFunction(sum);
            }
            if (property.PropertyType == typeof(DateTime))
            {
                return Items.Min(i => (DateTime)property.GetValue(i));
            }
            if (property.PropertyType == typeof(DateTime?))
            {
                return Items.Min(i => (DateTime?)property.GetValue(i));
            }
            throw new NotImplementedException(String.Format("Unhandled type {0}", property.PropertyType.Name));
        }

        public object Max(PropertyInfo property, DecimalRounding rounding)
        {
            if (!Items.Any())
            {
                return 0;
            }

            if (property.PropertyType == typeof(int))
            {
                return Items.Max(i => (int)property.GetValue(i));
            }
            if (property.PropertyType == typeof(int?))
            {
                return Items.Max(i => (int?)property.GetValue(i) ?? 0);
            }
            if (property.PropertyType == typeof(double))
            {
                Func<double, double> roundFunction = rounding.GetDoubleRoundingFunction();
                double sum = Items.Max(i => (double)property.GetValue(i));
                return roundFunction(sum);
            }
            if (property.PropertyType == typeof(double?))
            {
                Func<double, double> roundFunction = rounding.GetDoubleRoundingFunction();
                double sum = Items.Max(i => (double?)property.GetValue(i) ?? 0);
                return roundFunction(sum);
            }
            if (property.PropertyType == typeof(decimal))
            {
                Func<decimal, decimal> roundFunction = rounding.GetDecimalRoundingFunction();
                decimal sum = Items.Max(i => (decimal)property.GetValue(i));
                return roundFunction(sum);
            }
            if (property.PropertyType == typeof(decimal?))
            {
                Func<decimal, decimal> roundFunction = rounding.GetDecimalRoundingFunction();
                decimal sum = Items.Max(i => (decimal?)property.GetValue(i) ?? 0);
                return roundFunction(sum);
            }
            if (property.PropertyType == typeof(DateTime))
            {
                return Items.Max(i => (DateTime)property.GetValue(i));
            }
            if (property.PropertyType == typeof(DateTime?))
            {
                return Items.Max(i => (DateTime?)property.GetValue(i));
            }
            throw new NotImplementedException(String.Format("Unhandled type {0}", property.PropertyType.Name));
        }

        IEnumerable<object> ISelection.GetItems() => Items;
    }
}
