using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Helpers
{
	public static class FieldHelper
	{
		public static PropertySelector Parse(Type entityType, string fields)
		{
			var selector = PropertySelector.NewFromType(entityType);

			Parse(selector, fields);

			return selector;
		}
		private static void Parse(PropertySelector selector, string fields)
		{
			fields = fields ?? "";
			fields = fields.Replace(", ", ",");

			var list = FieldExpansionHelper.Expand(fields);

			foreach (var field in list)
			{
				selector.Parse(field);
			}
		}
		public static PropertySelector ParseAllProperties(Type entityType)
		{
			var fields = String.Join(", ", entityType.GetProperties().Select(p => p.Name).ToArray());

			return Parse(entityType, fields);
		}
	}
}
