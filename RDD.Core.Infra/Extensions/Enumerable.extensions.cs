using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public static class EnumerableExtensions
	{
		public static Dictionary<string, IEnumerable<string>> ToLowerDotDictionary(this IEnumerable<string> list)
		{
			return list.Where(el => !String.IsNullOrEmpty(el)).Select(el => el.ToLower()).GroupBy(el => el.Split('.')[0]).ToDictionary(g => g.Key, g => g.All(s => s.Contains('.')) ? g.Select(s => String.Join(".", s.Split('.').Skip(1).ToArray())) : (IEnumerable<string>)null);
		}
	}
}
