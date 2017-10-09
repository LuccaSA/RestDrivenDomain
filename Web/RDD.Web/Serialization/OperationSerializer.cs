using RDD.Domain.Helpers;
using RDD.Domain.Models;
using System;
using System.Linq.Expressions;

namespace RDD.Web.Serialization
{
	public class OperationSerializer : PropertySerializer
	{
		public OperationSerializer()
			: base() { }
		public OperationSerializer(IEntitySerializer serializer)
			: base(serializer) { }

		public override object SerializeProperty(object entity, PropertySelector field)
		{
			Expression<Func<Operation, Func<string>>> exp = o => o.CultureLabel;

			if (field.IsEqual(exp))
			{
				return ((Operation)entity).CultureLabel();
			}

			return base.SerializeProperty(entity, field);
		}
	}
}