using Inflector;
using System.Collections.Generic;
using System.Globalization;

namespace RDD.Web.Serialization
{
	public class PluralizationService
	{
	    private readonly Dictionary<string, string> _pluralsByName;

		public PluralizationService()
		{
			Inflector.Inflector.SetDefaultCultureFunc = () => new CultureInfo("en-US");

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
