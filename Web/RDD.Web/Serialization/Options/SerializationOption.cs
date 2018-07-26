using RDD.Domain;
using System.Collections.Generic;

namespace RDD.Web.Serialization.Options
{
    public class SerializationOption
    {
        public IReadOnlyCollection<IPropertySelector> Selectors { get; set; }
    }
}