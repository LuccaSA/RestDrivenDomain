using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using Newtonsoft.Json.Serialization;
using RDD.Domain;
using RDD.Domain.Models.Querying;
using RDD.Web.Serialization;
using RDD.Domain.Exceptions;
using NExtends.Primitives;
using HttpContextWrapper = RDD.Infra.Contexts.HttpContextWrapper;
using RDD.Domain.Helpers;

namespace RDD.Web.Helpers
{
	public class ApiHelper<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>, new()
		where TKey : IEquatable<TKey>
	{
		private Query<TEntity> _query { get; set; }
		private IContractResolver _jsonResolver { get; set; }
		private IWebContext _webContext { get; set; }

		public ApiHelper(IWebContext webContext, Query<TEntity> query = null, IContractResolver jsonResolver = null)
		{
			_webContext = webContext;
			_query = query ?? new Query<TEntity>();
			_jsonResolver = jsonResolver ?? new CamelCasePropertyNamesContractResolver();
		}

		public virtual Query<TEntity> CreateQuery(HttpVerb verb, bool isCollectionCall = true)
		{
			var query = _query.Parse(_webContext, isCollectionCall);
			query.Verb = verb;

			return query;
		}

		protected virtual ICollection<Expression<Func<TEntity, object>>> IgnoreList()
		{
			return new HashSet<Expression<Func<TEntity, object>>>();
		}

		public JsonMediaTypeFormatter GetFormatter()
		{
			return JsonApiFormatter.GetInstance(_webContext, GetJsonResolver());
		}

		private IContractResolver GetJsonResolver()
		{
			return _jsonResolver;
		}

		public List<PostedData> InputObjectsFromIncomingHTTPRequest(IRequestMessage request)
		{
			var objects = new List<PostedData>();

			var contentType = request.ContentType.Split(';')[0];
			var rawInput = request.Content;

			switch (contentType)
			{
				case "application/x-www-form-urlencoded":
				case "text/plain":
					objects.Add(PostedData.ParseDictionary(request.ContentAsFormDictionnary));
					break;

				//ce content-type est le seul à pouvoir envoyer plus qu'un seul formulaire
				case "application/json":
					if (rawInput.StartsWith("[")) //soit une collection
					{
						objects = PostedData.ParseJSONArray(rawInput).subs.Values.ToList();
					}
					else //soit un élément simple
					{
						objects.Add(PostedData.ParseJSON(rawInput));
					}
					break;

				//On récupère le fichier via HttpPostedFiles, donc on n'utilise pas formParams
				case "multipart/form-data":
					objects.Add(new PostedData()); //Faut quand même qu'il y en ait 1 dans la liste
					break;

				default:
					throw new HttpLikeException(HttpStatusCode.UnsupportedMediaType, String.Format("Unsupported media type {0}", contentType));
			}
			return objects;
		}

		/// <summary>
		/// This allows to explicitly not take into account elements that are not supposed to become auto-generated query members
		/// </summary>
		/// <param name="filters"></param>
		public void IgnoreFilters(params string[] filters)
		{
			_query.IgnoreFilters(filters);
		}
	}
}