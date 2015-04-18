using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public static class StringExtensions
	{
		public static string[] ToLower(this string[] array)
		{
			return array.Select(s => s.ToLower()).ToArray();
		}
		public static String ToJSON(this String s)
		{
			return JsonConvert.SerializeObject(s);
		}
		public static object ChangeType(this string value, Type propertyType, IFormatProvider culture)
		{
			if (propertyType.IsEnum)
			{
				return Enum.Parse(propertyType, value, true);
			}
			else
			{
				return Convert.ChangeType(value, propertyType, culture);
			}
		}
	}
}
