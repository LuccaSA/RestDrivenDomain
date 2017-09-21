using Inflector;
using System.Collections.Generic;

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
				_pluralsByName[name] = InflectorExtensions.Pluralize(name);
			}

			return _pluralsByName[name];
		}
	}
}
