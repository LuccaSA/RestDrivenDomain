using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.Querying
{
    public class FieldsParser
    {
        public IEnumerable<Field> ParseFields<TEntity>(Dictionary<string, string> parameters, bool isCollectionCall)
        {
            if (parameters.ContainsKey(Reserved.fields.ToString()))
            {
                return ParseFields<TEntity>(parameters[Reserved.fields.ToString()]);
            }
            else if (!isCollectionCall)
            {
                return ParseAllProperties<TEntity>();
            }

            return new HashSet<Field<TEntity>>();
        }

        public IEnumerable<Field> ParseFields<TEntity>(string fields)
        {
            var expandedFields = new FieldExpansionHelper().Expand(fields);

            return ParseFields<TEntity>(expandedFields);
        }

        public IEnumerable<Field> ParseAllProperties<TEntity>()
        {
            return ParseAllProperties(typeof(TEntity));
        }

        public IEnumerable<Field> ParseAllProperties(Type entityType)
        {
            var fields = entityType.GetProperties().Select(p => p.Name).ToList();

            return ParseFields(entityType, fields);
        }

        public IEnumerable<Field> ParseFields<TEntity>(List<string> fields)
        {
            return ParseFields(typeof(TEntity), fields);
        }
        public virtual IEnumerable<Field> ParseFields(Type entityType, List<string> fields)
        {
            var result = new HashSet<Field>();

            foreach (var item in fields)
            {
                if (!item.StartsWith("collection."))
                {
                    var selector = new PropertySelector(entityType);
                    selector.Parse(item);
                    var field = new Field(entityType, selector);
                    result.Add(field);
                }
            }

            return result;
        }

        public static Field NewFromType(Type entityType)
        {
            return (Field)typeof(Field).GetMethod("New").MakeGenericMethod(entityType).Invoke(null, new object[] { });
        }

    }
}