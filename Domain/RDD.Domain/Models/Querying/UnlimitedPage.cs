namespace RDD.Domain.Models.Querying
{
	internal class UnlimitedPage : Page
	{
		new internal const int MAX_LIMIT = 100000000; //100 millions ~= infinite

		public UnlimitedPage()
			: base(0, MAX_LIMIT, MAX_LIMIT) { }
	}
}
