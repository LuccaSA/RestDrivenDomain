using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Json;
using RDD.Domain.Models.Querying;
using RDD.Web.Models;
using RDD.Web.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Web.Helpers
{
    public class ApiHelper<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
    {
        private readonly IHttpContextHelper _httpContextHelper;
        private readonly IQueryFactory _queryFactory;

        public ApiHelper(IHttpContextHelper httpContextHelper, IQueryFactory queryFactory)
        {
            _httpContextHelper = httpContextHelper;
            _queryFactory = queryFactory;
        }

        public virtual Query<TEntity> CreateQuery(HttpVerbs verb)
        {
            Query<TEntity> query = _queryFactory.NewFromHttpRequest<TEntity, TKey>();
            query.Verb = verb;
            return query;
        }

        protected virtual ICollection<Expression<Func<TEntity, object>>> IgnoreList() => new HashSet<Expression<Func<TEntity, object>>>();

        public virtual ICandidate<TEntity, TKey> CreateCandidate()
        {
            string rawInput = _httpContextHelper.GetContent();
            var jsonObject = ParseIntoObject(rawInput);

            return new Candidate<TEntity, TKey>(rawInput, jsonObject);
        }

        public virtual IEnumerable<ICandidate<TEntity, TKey>> CreateCandidates()
        {
            string rawInput = _httpContextHelper.GetContent();
            var jsonObject = ParseIntoArray(rawInput);

            //TODO : découper rawInput en autant d'éléments JSON
            return jsonObject.Content.Select(el => new Candidate<TEntity, TKey>(rawInput, el as JsonObject));
        }

        public JsonArray ParseIntoArray(string rawInput)
        {
            var element = Parse(rawInput);
            var result = element as JsonArray;
            return result ?? new JsonArray { Content = new List<IJsonElement> { element } };
        }

        public JsonObject ParseIntoObject(string rawInput)
        {
            var element = Parse(rawInput) as JsonObject;
            if (element == null)
                throw new BadRequestException($"Unsupported data type. Please send a JSON object. Input sent {rawInput}");

            return element;
        }

        IJsonElement Parse(string rawInput)
        {
            string contentType = _httpContextHelper.GetContentType();

            switch (contentType)
            {
                case "application/x-www-form-urlencoded":
                case "text/plain":
                    //TODO : gérer ces 2 cases
                    throw new NotImplementedException();

                //ce content-type est le seul à pouvoir envoyer plus qu'un seul formulaire
                case "application/json":
                    return new JsonParser().Parse(rawInput);

                //On récupère le fichier via HttpPostedFiles, donc on n'utilise pas formParams
                case "multipart/form-data":
                    return new JsonObject();

                default:
                    throw new UnsupportedContentTypeException($"Unsupported content type {contentType}");
            }
        }
    }
}