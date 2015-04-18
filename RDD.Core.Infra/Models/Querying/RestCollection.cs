using RDD.Infra.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Querying
{
	public class RestCollection<IEntity, TKey>
	{
		public ICollection<IEntity> Items { get; set; }

		private int? _count;

		/// <summary>
		/// Count is either set explicitely, or calculated at first access
		/// </summary>
		public int Count
		{
			get { return (int)(_count.HasValue ? _count.Value : _count = Items.Count); }
			set { _count = value; }
		}

		public RestCollection()
		{
			Items = new HashSet<IEntity>();
		}

		public object Sum(PropertyInfo property, string[] sumParameters)
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
				Func<double, double> roundFunction = GetRoundingFunction(sumParameters);
				var sum = Items.Sum(i => (double)property.GetValue(i));
				return roundFunction(sum);
			}
			else if (property.PropertyType == typeof(double?))
			{
				Func<double, double> roundFunction = GetRoundingFunction(sumParameters);
				var sum = Items.Sum(i => (double?)property.GetValue(i) ?? 0);
				return roundFunction(sum);
			}
			else
			{
				throw new HttpLikeException(HttpStatusCode.NotImplemented, String.Format("Unhandled type {0}", property.PropertyType.Name));
			}
		}

		private static Func<double, double> GetRoundingFunction(string[] sumParameters)
		{
			int decimals;
			Func<double, double> func;

			if (sumParameters.Length > 1)
			{
				if (!int.TryParse(sumParameters[1], out decimals))
				{
					throw new HttpLikeException(HttpStatusCode.BadRequest, string.Format("Incorrect query for collection.sum, incorrect number of decimals : '{0}'", sumParameters[1]));
				}
			}
			else
			{
				decimals = 2;
			}

			if (sumParameters.Length > 0)
			{

				switch (sumParameters[0])
				{
					case "round":
						func = (doubleValue) => Math.Round(doubleValue, decimals, MidpointRounding.AwayFromZero);
						break;
					case "roundeven":
						func = (doubleValue) => Math.Round(doubleValue, decimals, MidpointRounding.ToEven);
						break;
					case "ceiling":
						func = (doubleValue) => Math.Ceiling(doubleValue);
						break;
					case "floor":
						func = (doubleValue) => Math.Floor(doubleValue);
						break;
					default:
						throw new HttpLikeException(HttpStatusCode.BadRequest, string.Format("Incorrect query for collection.sum, unknown rounding function '{0}'", sumParameters[0]));
				}

			}
			else
			{
				func = (doubleValue) => doubleValue;
			}
			return func;
		}
	}
}

