using RDD.Domain;
using RDD.Domain.Models.Querying;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.Serialization.Options
{
    public class SerializationOption
    {
        public IReadOnlyCollection<IPropertySelector> Selectors { get; set; }

        public SerializationOption() { }
        public SerializationOption(IEnumerable<Field> fields)
        {
            Selectors = fields.Select(f => f.EntitySelector).ToList();
        }
    }
}