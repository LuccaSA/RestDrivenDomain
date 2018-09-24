using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RDD.Domain;
using RDD.Web.Serialization.UrlProviders;

namespace RDD.Web.Serialization
{
    /// <summary>
    /// Json contract resolver used to select serialized properties based on query field selection
    /// </summary>
    public class SelectiveContractResolver : DefaultContractResolver
    {
        // Singleton for UrlService resolution (newtonsoft contract resolver aren't injectable)
        internal static IUrlProvider UrlProvider { get; set; }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            property.ShouldSerialize = instance => SelectiveSerialisationContext.Current.IsCurrentNodeDefined(property.PropertyName);
            return property;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);

            if (typeof(IEntityBase).IsAssignableFrom(type))
            {
                string urlName = "Url";
                if (props.Any(i => i.PropertyName == "Url"))
                {
                    urlName = "UrlNavigation";
                }
                props.Add(new JsonProperty
                {
                    DeclaringType = type,
                    PropertyType = typeof(string),
                    PropertyName = urlName,
                    ValueProvider = new UrlValueProvider(UrlProvider),
                    Readable = true,
                    Writable = false
                });
            }

            return props;
        }
    }

    public class UrlValueProvider : IValueProvider
    {
        private readonly IUrlProvider _urlProvider;

        public UrlValueProvider(IUrlProvider urlProvider)
        {
            _urlProvider = urlProvider;
        }

        public void SetValue(object target, object value)
        {
            throw new NotImplementedException();
        }

        public object GetValue(object target)
        {
            var uri = _urlProvider.GetEntityApiUri(target as IPrimaryKey);
            return uri.ToString();
        }
    }
}