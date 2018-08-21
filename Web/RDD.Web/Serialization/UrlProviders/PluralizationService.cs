using System;

namespace RDD.Web.Serialization.UrlProviders
{
    public class PluralizationService : IPluralizationService
    {
        private readonly Inflector.Inflector _inflector;

        public PluralizationService(Inflector.Inflector inflector)
        {
            _inflector = inflector ?? throw new ArgumentNullException(nameof(inflector));
        }

        public string GetPlural(string name) => _inflector.Pluralize(name);
    }
}