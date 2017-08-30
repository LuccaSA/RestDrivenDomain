using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http.Formatting;
using Newtonsoft.Json.Serialization;
using RDD.Domain;
using RDD.Web.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Models;
using System.Web.Http;

namespace RDD.Web.Controllers
{
	public partial class ReadOnlyWebApiController<TCollection, TEntity, TKey> : ApiController
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