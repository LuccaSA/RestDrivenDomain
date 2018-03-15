using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;

namespace RDD.Web.Querying
{
    public class CollectionFieldsParser : FieldsParser
    {
        public override IEnumerable<Field> ParseFields(Type entityType, List<string> fields)
        {
            var result = new HashSet<Field>();

            foreach (var item in fields)
            {
                if (item.StartsWith("collection."))
                {
                    var selector = new PropertySelector(entityType);
                    selector.Parse(item);
                    var field = new Field(entityType, selector);
                    result.Add(field);
                }
            }

            return result;
        }
    }
}