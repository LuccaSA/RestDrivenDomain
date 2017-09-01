using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Web.Helpers;
using System;

namespace RDD.Web.Controllers
{
	public partial class ReadOnlyWebApiController<TCollection, TEntity, TKey> : ControllerBase
		where TCollection : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		protected IWebContext _webContext;
		protected IExecutionContext _execution;
		protected Func<IStorageService> _newStorage;
		protected IStorageService _storage;
		protected TCollection _collection;
		protected IEntitySerializer _serializer;
		protected ApiHelper<TEntity, TKey> ApiHelper { get; set; }

		public ReadOnlyWebApiController(IWebContext webContext, IExecutionContext execution, Func<IStorageService> newStorage, IEntitySerializer serializer, Query<TEntity> query = null, IContractResolver jsonResolver = null)
		{
			_webContext = webContext;
			_execution = execution;
			_newStorage = newStorage;
			_storage = _newStorage();
			_serializer = serializer;

			ApiHelper = new ApiHelper<TEntity, TKey>(_webContext, query, jsonResolver);
		}
	}
}