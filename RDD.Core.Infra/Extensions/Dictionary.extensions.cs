using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public static class DictionaryExtensions
	{
		public static bool ContainsKey<T>(this Dictionary<string, T> dic, Enum enumKey)
		{
			return dic.ContainsKey(enumKey.ToString());
		}
	}
}
