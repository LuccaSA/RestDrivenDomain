using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.Querying
{
    public class FieldsParser
    {
        public Field<TEntity> ParseFields<TEntity>(Dictionary<string, string> parameters, bool isCollectionCall)
        {
            if (parameters.ContainsKey(Reserved.fields.ToString()))
            {
                return ParseFields<TEntity>(parameters[Reserved.fields.ToString()]);
            }
            else if (!isCollectionCall)
            {
                return ParseAllProperties<TEntity>();
            }

            return new Field<TEntity>();
        }

        public Field<TEntity> ParseFields<TEntity>(string fields)
        {
            var expandedFields = new FieldExpansionHelper().Expand(fields);

            return ParseFields<TEntity>(expandedFields);
        }

        public Field<TEntity> ParseAllProperties<TEntity>()
        {
            var fields = typeof(TEntity).GetProperties().Select(p => p.Name).ToList();

            return ParseFields<TEntity>(fields);
        }

        public virtual Field<TEntity> ParseFields<TEntity>(List<string> fields)
        {
            var field = new Field<TEntity>();
            foreach (var item in fields)
            {
                if (!item.StartsWith("collection."))
                {
                    field.EntitySelector.Parse(item);
                }
            }
            return field;
        }

        public static Field NewFromType(Type entityType)
        {
            return (Field)typeof(Field).GetMethod("New").MakeGenericMethod(entityType).Invoke(null, new object[] { });
        }

    }
}