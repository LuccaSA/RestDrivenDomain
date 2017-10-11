using Newtonsoft.Json.Serialization;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Infra;
using RDD.Web.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;

namespace RDD.Web.Helpers
{
    public class ApiHelper<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>, new()
        where TKey : IEquatable<TKey>
    {
        private readonly QueryFactory<TEntity> _queryFactory = new QueryFactory<TEntity>();
        private IContractResolver _jsonResolver { get; }

        public IWebContextWrapper WebContextWrapper { get; }
        public IExecutionContext Execution { get; }
        public IEntitySerializer Serializer { get; }

        public ApiHelper(IContractResolver jsonResolver, IWebContextWrapper webContextWrapper, IExecutionContext execution, IEntitySerializer serializer)
        {
            _jsonResolver = jsonResolver;
            WebContextWrapper = webContextWrapper;
            Execution = execution;
            Serializer = serializer;
        }

        public virtual Query<TEntity> CreateQuery(HttpVerb verb, bool isCollectionCall = true)
        {
            var query = _queryFactory.FromWebContext(WebContextWrapper, isCollectionCall);
            query.Verb = verb;

            return query;
        }

        protected virtual ICollection<Expression<Func<TEntity, object>>> IgnoreList()
        {
            return new HashSet<Expression<Func<TEntity, object>>>();
        }

        private IContractResolver GetJsonResolver()
        {
            return _jsonResolver;
        }

        public List<PostedData> InputObjectsFromIncomingHTTPRequest()
        {
            var objects = new List<PostedData>();
            var contentType = WebContextWrapper.ContentType.Split(';')[0];
            var rawInput = WebContextWrapper.Content;

            switch (contentType)
            {
                case "application/x-www-form-urlencoded":
                case "text/plain":
                    {
                        var dictionaryInput = String.IsNullOrEmpty(rawInput) ? new Dictionary<string, string>() : rawInput.Split('&').Select(s => s.Split('=')).ToDictionary(p => p[0], p => p[1]);
                        objects.Add(PostedData.ParseDictionary(dictionaryInput));
                        break;
                    }
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
            _queryFactory.IgnoreFilters(filters);
        }
    }
}