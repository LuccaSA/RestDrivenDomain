using RDD.Domain.Models.Querying;
using System.Collections.Generic;

namespace RDD.Web.Querying
{
    public class CollectionFieldsParser : FieldsParser
    {
        public override Field<TEntity> ParseFields<TEntity>(List<string> fields)
        {
            var field = new Field<TEntity>();
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