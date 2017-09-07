using RDD.Domain.Models.Querying;
using RDD.Domain.Models.StorageQueries.Includers;

namespace RDD.Domain.Models.Convertors.Includers
{
	public interface IIncluderConvertor<T> where T : class
	{
		IIncluder<T> ConverterToIncluder(Query<T> request);
	}
}