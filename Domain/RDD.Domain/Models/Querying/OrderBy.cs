namespace RDD.Domain.Models.Querying
{
	public enum SortDirection { Ascending, Descending };

	public class OrderBy
	{
		public string Field { get; set; }
		public SortDirection Direction { get; set; }
	}
}
