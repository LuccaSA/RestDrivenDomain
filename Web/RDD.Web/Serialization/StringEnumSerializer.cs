using RDD.Domain.Helpers;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NExtends.Primitives;
using NExtends.Primitives.Types;

namespace RDD.Web.Serialization
{
	public class StringEnumSerializer : PropertySerializer
	{
		public StringEnumSerializer() { }
		public StringEnumSerializer(IEntitySerializer serializer) : base(serializer) { }

		public override object SerializeProperty(object entity, PropertySelector field)
		{
			var obj = base.SerializeProperty(entity, field);
			if (obj != null && field != null && field.EntityType != null && field.EntityType.IsEnum)
			{
				if (field.Lambda != null && field.Lambda.ReturnType.IsEnumerableOrArray())
				{
					obj = ((IEnumerable)obj).Cast<object>().Select(o => o.ToString());
				}
				else
				{
					obj = obj.ToString();
				}
			}
			return obj;
		}
	}
}