using RDD.Domain.Models.Convertors.Filters;
using RDD.Domain.Models.Convertors.Includers;
using RDD.Domain.Models.Convertors.Orderers;
using RDD.Domain.Models.Convertors.Pagers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Models.StorageQueries;
using System.Diagnostics;

namespace RDD.Domain.Models.Convertors
{
	public class QueryConvertor<T> : IQueryConvertor<T> where T : class
	{
		IFilterConvertor<T> _filterConvertor;
		IIncluderConvertor<T> _includerConvertor;
		IOrdererConvertor<T> _ordererConvertor;
		IPagerConvertor<T> _pagerConvertor;

		public QueryConvertor(IFilterConvertor<T> filterConvertor, IIncluderConvertor<T> includerConvertor, IOrdererConvertor<T> ordererConvertor, IPagerConvertor<T> pagerConvertor)
		{
			_filterConvertor = filterConvertor;
			_includerConvertor = includerConvertor;
			_ordererConvertor = ordererConvertor;
			_pagerConvertor = pagerConvertor;
		}

		public IStorageQuery<T> Convert(Query<T> request) => Convert(request, new Stopwatch());
		public IStorageQuery<T> Convert(Query<T> request, Stopwatch watch)
		{
			var filter = _filterConvertor.ConverterToFilter(request);
			var includer = _includerConvertor.ConverterToIncluder(request);
			var orderer = _ordererConvertor.ConverterToOrderer(request);
			var pager = _pagerConvertor.ConverterToPager(request);

			return new StorageQuery<T>(watch, filter, includer, orderer, pager);
		}
	}
}