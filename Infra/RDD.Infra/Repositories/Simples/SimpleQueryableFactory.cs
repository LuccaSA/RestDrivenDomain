using RDD.Domain.Models.Rights;
using RDD.Domain.Models.StorageQueries;
using RDD.Domain.Models.StorageQueries.Filters;
using RDD.Domain.Models.StorageQueries.Includers;
using RDD.Domain.Models.StorageQueries.Orderers;
using RDD.Domain.Models.StorageQueries.Pagers;
using RDD.Infra.Services;
using System.Linq;

namespace RDD.Infra.Repositories.Simples
{
	public class SimpleQueryableFactory<T> where T : class
	{
		protected IStorageService _storageService;
		IReadRightService<T> _rightService;

		public SimpleQueryableFactory(IStorageService storageService, IReadRightService<T> rightService)
		{
			_storageService = storageService;
			_rightService = rightService;
		}

		protected virtual IQueryable<T> PrepareQuery(IStorageQuery<T> query)
		{
			return _storageService.Set<T>()
				.ApplyRights(_rightService)
				.ApplyFilter(query.Filter)
				.ApplyOrder(query.Orderer)
				.ApplyPaging(query.Pager)
				.ApplyInclude(query.Includer);
		}
	}

	public static class ExtensionsMethods
	{
		public static IQueryable<T> ApplyRights<T>(this IQueryable<T> source, IReadRightService<T> rightService) => rightService.ApplyFilter(source);
		public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> source, IFilter<T> rightService) => rightService.ApplyFilter(source);
		public static IQueryable<T> ApplyOrder<T>(this IQueryable<T> source, IOrderer<T> orderer) => orderer.Order(source);
		public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> source, IPager pager) => pager.Page(source);
		public static IQueryable<T> ApplyInclude<T>(this IQueryable<T> source, IIncluder<T> selector) => selector.ApplyInclude(source);
	}
}
