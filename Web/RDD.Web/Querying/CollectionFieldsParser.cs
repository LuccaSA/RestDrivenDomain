using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;

namespace RDD.Web.Querying
{
    public class CollectionFieldsParser : FieldsParser
    {
        public override Field ParseFields(Type entityType, List<string> fields)
        {
            var field = new Field(entityType);

            foreach (var item in fields)
            {
                if (item.StartsWith("collection."))
                {
                    field.EntitySelector.Parse(item);
                }
            }
            return field;
        }
    }
}