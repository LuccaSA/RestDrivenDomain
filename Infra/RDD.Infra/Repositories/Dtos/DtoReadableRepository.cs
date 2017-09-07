using RDD.Domain.Contracts;
using RDD.Domain.Models.Rights;
using RDD.Domain.Models.StorageQueries;
using RDD.Infra.Repositories.Mappings;
using RDD.Infra.Repositories.Simples;
using RDD.Infra.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Infra.Repositories.Dtos
{
	public class DtoReadableRepository<T, TInfra> : SimpleQueryableFactory<T>, IReadableRepository<T> 
		where TInfra : class
		where T : class
	{
		protected IMapper<TInfra, T> _outMapper;		
		IQueryableConvertor<T, TInfra> _convertor;

		public DtoReadableRepository(IStorageService storageService, IReadRightService<T> rightService, IQueryableConvertor<T, TInfra> convertor, IMapper<TInfra, T> outMapper)
			 : base(storageService, rightService)
		{
			_outMapper = outMapper;
			_convertor = convertor;
		}

		public int Count(IStorageQuery<T> query) => EvaluateQuery(query, q => q.Count());
		public bool HasAny(IStorageQuery<T> query) => EvaluateQuery(query, q => q.Any());
		public IEnumerable<T> Get(IStorageQuery<T> query) => EvaluateQuery(query, q => q.AsEnumerable()).Select(_outMapper.Map);

		protected TResult EvaluateQuery<TResult>(IStorageQuery<T> query, Func<IQueryable<TInfra>, TResult> method)
		{
			var queryable = _convertor.Convert(PrepareQuery(query));

			query.Watch.Start();
			var result = method(queryable);
			query.Watch.Stop();

			return result;
		}
	}
}