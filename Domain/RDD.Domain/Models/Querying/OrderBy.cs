using RDD.Domain.Helpers;

namespace RDD.Domain.Models.Querying
{
	public enum SortDirection { Ascending, Descending };

	public class OrderBy<TEntity>
		where TEntity : class, IEntityBase
	{
		public PropertySelector<TEntity> Property { get; }
		public SortDirection Direction { get; }

		public OrderBy(PropertySelector<TEntity> property, SortDirection direction)
		{
			Property = property;
			Direction = direction;
		}
	}
}
