using RDD.Domain.Models.Querying;
using RDD.Domain.Models.StorageQueries.Orderers;

namespace RDD.Domain.Models.Convertors.Orderers
{
	public interface IOrdererConvertor<T> where T : class
	{
		IOrderer<T> ConverterToOrderer(Query<T> request);
	}
}