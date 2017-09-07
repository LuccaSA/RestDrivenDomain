using RDD.Domain.Models.Querying;
using RDD.Domain.Models.StorageQueries.Includers;

namespace RDD.Domain.Models.Convertors.Includers
{
	class IncluderConvertor<T> : IIncluderConvertor<T> where T : class
	{
		public IIncluder<T> ConverterToIncluder(Query<T> request) => request.Includes.GetIncluder();
	}
}