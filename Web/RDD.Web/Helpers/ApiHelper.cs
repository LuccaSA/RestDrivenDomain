using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Serialization;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Web.Querying;

namespace RDD.Web.Helpers
{
    public class ApiHelper<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>, new()
        where TKey : IEquatable<TKey>
    {
        public ApiHelper(IContractResolver jsonResolver, IExecutionContext execution, IEntitySerializer serializer, IHttpContextAccessor httpContextAccessor)
        {
            _jsonResolver = jsonResolver;
            _httpContextAccessor = httpContextAccessor;
            Execution = execution;
            Serializer = serializer;
        }

        private readonly IContractResolver _jsonResolver;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly QueryFactory<TEntity> _queryFactory = new QueryFactory<TEntity>();

        //public IWebContextWrapper WebContextWrapper { get; }
        public IExecutionContext Execution { get; }
        public IEntitySerializer Serializer { get; }

        public virtual Query<TEntity> CreateQuery(HttpVerbs verb, bool isCollectionCall = true)
        {
            Query<TEntity> query = _queryFactory.FromWebContext(_httpContextAccessor.HttpContext, isCollectionCall);
            query.Verb = verb;
            return query;
        }

        protected virtual ICollection<Expression<Func<TEntity, object>>> IgnoreList() => new HashSet<Expression<Func<TEntity, object>>>();

        private IContractResolver GetJsonResolver() => _jsonResolver;

        public List<PostedData> InputObjectsFromIncomingHttpRequest()
        {
            var objects = new List<PostedData>();
            string contentType = _httpContextAccessor.HttpContext.Request.ContentType.Split(';')[0];
            string rawInput = _httpContextAccessor.HttpContext.GetContent();

            switch (contentType)
            {
                case "application/x-www-form-urlencoded":
                case "text/plain":
                {
                    Dictionary<string, string> dictionaryInput = string.IsNullOrEmpty(rawInput) ? new Dictionary<string, string>() : rawInput.Split('&').Select(s => s.Split('=')).ToDictionary(p => p[0], p => p[1]);
                    objects.Add(PostedData.ParseDictionary(dictionaryInput));
                    break;
                }
                //ce content-type est le seul à pouvoir envoyer plus qu'un seul formulaire
                case "application/json":
                    if (rawInput.StartsWith("[")) //soit une collection
                    {
                        objects = PostedData.ParseJsonArray(rawInput).Subs.Values.ToList();
                    }
                    else //soit un élément simple
                    {
                        objects.Add(PostedData.ParseJson(rawInput));
                    }
                    break;

                //On récupère le fichier via HttpPostedFiles, donc on n'utilise pas formParams
                case "multipart/form-data":
                    objects.Add(new PostedData()); //Faut quand même qu'il y en ait 1 dans la liste
                    break;

                default:
                    throw new HttpLikeException(HttpStatusCode.UnsupportedMediaType, string.Format("Unsupported media type {0}", contentType));
            }
            return objects;
        }

        /// <summary>
        /// This allows to explicitly not take into account elements that are not supposed to become auto-generated query members
        /// </summary>
        /// <param name="filters"></param>
        public void IgnoreFilters(params string[] filters)
        {
            _queryFactory.IgnoreFilters(filters);
        }
    }
}