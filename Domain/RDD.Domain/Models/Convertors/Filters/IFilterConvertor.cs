using RDD.Domain.Models.Querying;
using RDD.Domain.Models.StorageQueries.Filters;

namespace RDD.Domain.Models.Convertors.Filters
{
	public interface IFilterConvertor<T> where T : class
	{
		IFilter<T> ConverterToFilter(Query<T> request);
	}
}