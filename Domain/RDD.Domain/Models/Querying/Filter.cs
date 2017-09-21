using RDD.Domain.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RDD.Domain.Models.Querying
{
	public class Filter<TEntity>
	{
		public PropertySelector<TEntity> Property { get; private set; }
		public FilterOperand Operand { get; private set; }
		public IList Values { get; private set; }

		public Filter(PropertySelector<TEntity> property, FilterOperand operand, IList values)
		{
			Property = property;
			Operand = operand;
			Values = values;
		}

		public Filter(Expression<Func<TEntity, object>> property, FilterOperand operand, string value)
			: this(new PropertySelector<TEntity>(property), operand, new List<string>() { value }) { }
	}
}