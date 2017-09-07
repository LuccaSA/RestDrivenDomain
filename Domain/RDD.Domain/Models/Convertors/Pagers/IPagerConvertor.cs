using RDD.Domain.Models.Querying;
using RDD.Domain.Models.StorageQueries.Pagers;

namespace RDD.Domain.Models.Convertors.Pagers
{
	public interface IPagerConvertor<T> where T : class
	{
		IPager ConverterToPager(Query<T> request);
	}
}