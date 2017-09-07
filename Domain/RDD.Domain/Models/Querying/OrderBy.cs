using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Models.Querying
{
	public enum SortDirection { Ascending, Descending };

	public class OrderBy<TEntity>
		where TEntity : class, IEntityBase
	{
		public PropertySelector<TEntity> Property { get; private set; }
		public SortDirection Direction { get; private set; }

		public OrderBy(PropertySelector<TEntity> property, SortDirection direction)
		{
			Property = property;
			Direction = direction;
		}
	}
}
