using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Web.Serialization
{
	public class CultureSerializer : PropertySerializer
	{
		public CultureSerializer()
			: base() { }

		public CultureSerializer(IEntitySerializer serializer)
			: base(serializer) { }

		//public override object SerializeProperty(object entity, Domain.Helpers.PropertySelector field)
		//{
		//	Expression<Func<Culture, CultureInfo>> exp = c => c.RawCulture;

		//	if(field.IsEqual(exp))
		//	{
		//		return 
		//	}

		//	return base.SerializeProperty(entity, field);
		//}

		public override Dictionary<string, object> SerializeProperties(object entity, PropertySelector fields)
		{
			Expression<Func<Culture, CultureInfo>> exp = c => c.RawCulture;

			fields.Remove(exp);

			return base.SerializeProperties(entity, fields);
		}
	}
}
