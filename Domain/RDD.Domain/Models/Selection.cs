using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace RDD.Domain.Models
{
	public class Selection<TEntity> : ISelection<TEntity>
		where TEntity : class, IEntityBase
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
			else if (property.PropertyType == typeof(int?))
			{
				return Items.Sum(i => (int?)property.GetValue(i) ?? 0);
			}
			else if (property.PropertyType == typeof(double))
			{
				Func<double, double> roundFunction = rounding.GetDoubleRoundingFunction();
				var sum = Items.Sum(i => (double)property.GetValue(i));
				return roundFunction(sum);
			}
			else if (property.PropertyType == typeof(double?))
			{
				Func<double, double> roundFunction = rounding.GetDoubleRoundingFunction();
				var sum = Items.Sum(i => (double?)property.GetValue(i) ?? 0);
				return roundFunction(sum);
			}
			else if (property.PropertyType == typeof(decimal))
			{
				Func<decimal, decimal> roundFunction = rounding.GetDecimalRoundingFunction();
				var sum = Items.Sum(i => (decimal)property.GetValue(i));
				return roundFunction(sum);
			}
			else if (property.PropertyType == typeof(decimal?))
			{
				Func<decimal, decimal> roundFunction = rounding.GetDecimalRoundingFunction();
				var sum = Items.Sum(i => (decimal?)property.GetValue(i) ?? 0);
				return roundFunction(sum);
			}
			else if (property.PropertyType == typeof(TimeSpan))
			{
				return new TimeSpan(Items.Sum(i => ((TimeSpan)property.GetValue(i)).Ticks));
			}
			else
			{
				throw new HttpLikeException(HttpStatusCode.NotImplemented, String.Format("Unhandled type {0}", property.PropertyType.Name));
			}
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
			else if (property.PropertyType == typeof(int?))
			{
				return Items.Min(i => (int?)property.GetValue(i) ?? 0);
			}
			else if (property.PropertyType == typeof(double))
			{
				Func<double, double> roundFunction = rounding.GetDoubleRoundingFunction();
				var sum = Items.Min(i => (double)property.GetValue(i));
				return roundFunction(sum);
			}
			else if (property.PropertyType == typeof(double?))
			{
				Func<double, double> roundFunction = rounding.GetDoubleRoundingFunction();
				var sum = Items.Min(i => (double?)property.GetValue(i) ?? 0);
				return roundFunction(sum);
			}
			else if (property.PropertyType == typeof(decimal))
			{
				Func<decimal, decimal> roundFunction = rounding.GetDecimalRoundingFunction();
				var sum = Items.Min(i => (decimal)property.GetValue(i));
				return roundFunction(sum);
			}
			else if (property.PropertyType == typeof(decimal?))
			{
				Func<decimal, decimal> roundFunction = rounding.GetDecimalRoundingFunction();
				var sum = Items.Min(i => (decimal?)property.GetValue(i) ?? 0);
				return roundFunction(sum);
			}
			else if (property.PropertyType == typeof(DateTime))
			{
				return Items.Min(i => (DateTime)property.GetValue(i));
			}
			else if (property.PropertyType == typeof(DateTime?))
			{
				return Items.Min(i => (DateTime?)property.GetValue(i));
			}
			else
			{
				throw new HttpLikeException(HttpStatusCode.NotImplemented, String.Format("Unhandled type {0}", property.PropertyType.Name));
			}
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
			else if (property.PropertyType == typeof(int?))
			{
				return Items.Max(i => (int?)property.GetValue(i) ?? 0);
			}
			else if (property.PropertyType == typeof(double))
			{
				Func<double, double> roundFunction = rounding.GetDoubleRoundingFunction();
				var sum = Items.Max(i => (double)property.GetValue(i));
				return roundFunction(sum);
			}
			else if (property.PropertyType == typeof(double?))
			{
				Func<double, double> roundFunction = rounding.GetDoubleRoundingFunction();
				var sum = Items.Max(i => (double?)property.GetValue(i) ?? 0);
				return roundFunction(sum);
			}
			else if (property.PropertyType == typeof(decimal))
			{
				Func<decimal, decimal> roundFunction = rounding.GetDecimalRoundingFunction();
				var sum = Items.Max(i => (decimal)property.GetValue(i));
				return roundFunction(sum);
			}
			else if (property.PropertyType == typeof(decimal?))
			{
				Func<decimal, decimal> roundFunction = rounding.GetDecimalRoundingFunction();
				var sum = Items.Max(i => (decimal?)property.GetValue(i) ?? 0);
				return roundFunction(sum);
			}
			else if (property.PropertyType == typeof(DateTime))
			{
				return Items.Max(i => (DateTime)property.GetValue(i));
			}
			else if (property.PropertyType == typeof(DateTime?))
			{
				return Items.Max(i => (DateTime?)property.GetValue(i));
			}
			else
			{
				throw new HttpLikeException(HttpStatusCode.NotImplemented, String.Format("Unhandled type {0}", property.PropertyType.Name));
			}
		}
	}
}
