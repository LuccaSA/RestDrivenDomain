using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using RDD.Domain;
using RDD.Domain.Models;
using RDD.Infra;
using RDD.Web.Helpers;
using System;

namespace RDD.Web.Controllers
{
	public abstract partial class ReadOnlyWebApiController<TCollection, TEntity, TKey> : ControllerBase
		where TCollection : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		protected IExecutionContext _execution;
		protected IEntitySerializer _serializer;
		protected ICombinationsHolder _combinationHolder;
		protected IWebContext _webContext;
		protected ApiHelper<TEntity, TKey> _apiHelper;

		public ReadOnlyWebApiController(IExecutionContext execution, IEntitySerializer serializer, ICombinationsHolder combinationHolder, IWebContext webContext, IContractResolver jsonResolver = null)
		{
			_execution = execution;
			_serializer = serializer;
			_combinationHolder = combinationHolder;
			_webContext = webContext;
			_apiHelper = new ApiHelper<TEntity, TKey>(webContext, jsonResolver);
		}

		protected virtual IReadOnlyRestCollection<TEntity, TKey> GetReadOnlyCollection(IStorageService storage, IRepository<TEntity> repository)
		{
			return new ReadOnlyRestCollection<TEntity, TKey>(repository, _execution, _combinationHolder);
		}
		protected virtual IRestCollection<TEntity, TKey> GetCollection(IStorageService storage, IRepository<TEntity> repository)
		{
			return new RestCollection<TEntity, TKey>(repository, _execution, _combinationHolder);
		}
		protected abstract IStorageService GetStorage();
		protected abstract IRepository<TEntity> GetRepository(IStorageService storage);
	}
}