using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NExtends.Primitives;

namespace RDD.Web.Serialization
{
	public class PluralizationCacheService
	{
		readonly Dictionary<string, string> _pluralsByName;

		public PluralizationCacheService()
		{
			_pluralsByName = new Dictionary<string, string>();
		}

		public string GetPlural(string name)
		{
			if (!_pluralsByName.ContainsKey(name))
			{
				_pluralsByName[name] = name.Pluralize();
			}

			return _pluralsByName[name];
		}
	}
}
