using RDD.Domain.Helpers;

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
