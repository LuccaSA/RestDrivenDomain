using System;
using Newtonsoft.Json.Serialization;
using RDD.Domain;
using RDD.Web.Serialization.UrlProviders;

namespace RDD.Web.Serialization
{
    public class UrlValueProvider : IValueProvider
    {
        private readonly IUrlProvider _urlProvider;

        public UrlValueProvider(IUrlProvider urlProvider)
        {
            _urlProvider = urlProvider ?? throw new ArgumentNullException(nameof(urlProvider));
        }

        public void SetValue(object target, object value)
        {
            throw new NotImplementedException();
        }

        public object GetValue(object target)
        {
            if (target is IPrimaryKey key)
            {
                var uri = _urlProvider.GetEntityApiUri(key);
                return uri.ToString();
            }
            return null;
        }
    }
}