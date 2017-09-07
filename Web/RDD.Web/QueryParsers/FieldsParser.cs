using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.QueryParsers
{
	public class FieldsParser<T>
	{
		public Field<T> ParseFields(Dictionary<string, string> parameters, bool isCollectionCall)
		{
			if (parameters.ContainsKey(Reserved.fields.ToString()))
			{
				return ParseFields(parameters[Reserved.fields.ToString()]);
			}
			else if (!isCollectionCall)
			{
				return ParseFieldsAllProperties();
			}

			return new Field<T>();
		}

		public Field<T> ParseFields(string fields)
		{
			var expandedFields = new FieldExpansionHelper().Expand(fields);

			return ParseFields(expandedFields);
		}

		public Field<T> ParseFieldsAllProperties()
		{
			var fields = typeof(T).GetProperties().Select(p => p.Name).ToList();

			return ParseFields(fields);
		}

		public Field<T> ParseFields(List<string> fields)
		{
			var result = new Field<T>();
			foreach (var item in fields)
			{
				if (item.StartsWith("collection."))
				{
					result.CollectionSelector.Parse(item.Replace("collection.", ""));
				}
				else
				{
					result.EntitySelector.Parse(item);
				}
			}
			return result;
		}
	}
}