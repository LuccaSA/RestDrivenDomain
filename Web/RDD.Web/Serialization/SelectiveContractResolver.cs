using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RDD.Web.Serialization
{
    /// <summary>
    /// Json contract resolver used to select serialized properties based on query field selection
    /// </summary>
    public class SelectiveContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.ShouldSerialize = instance => SelectiveSerialisationContext.Current.IsCurrentNodeDefined(property.PropertyName);
            return property;
        }
    }
}