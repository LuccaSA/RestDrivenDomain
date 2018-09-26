using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            if (SelectiveSerialisationContext.Current != null)
            {
                property.ShouldSerialize = instance => SelectiveSerialisationContext.Current.IsCurrentNodeDefined(property.PropertyName);
            }
            return property;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);

            if (SelectiveSerialisationContext.Current != null && typeof(IEntityBase).IsAssignableFrom(type))
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
}