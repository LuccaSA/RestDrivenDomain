using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Rdd.Domain;
using Rdd.Domain.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdd.Web.Querying
{
    public class CandidateParser : ICandidateParser
    {
        private readonly IJsonParser _jsonParser;

        public CandidateParser(IJsonParser jsonParser)
        {
            _jsonParser = jsonParser ?? throw new ArgumentNullException(nameof(jsonParser));
        }

        public virtual async Task<ICandidate<TEntity, TKey>> ParseAsync<TEntity, TKey>(HttpRequest request)
            where TEntity : class, IPrimaryKey<TKey>
            => ParseMany<TEntity, TKey>(await GetContentAsync(request)).First();

        public virtual ICandidate<TEntity, TKey> Parse<TEntity, TKey>(string content)
            where TEntity : class, IPrimaryKey<TKey>
            => ParseMany<TEntity, TKey>(content).First();

        public virtual async Task<IEnumerable<ICandidate<TEntity, TKey>>> ParseManyAsync<TEntity, TKey>(HttpRequest request)
            where TEntity : class, IPrimaryKey<TKey>
            => ParseMany<TEntity, TKey>(await GetContentAsync(request));

        public virtual IEnumerable<ICandidate<TEntity, TKey>> ParseMany<TEntity, TKey>(string content)
            where TEntity : class, IPrimaryKey<TKey>
        {
            var token = JToken.Parse(content);
            switch (token)
            {
                case JArray array:
                    return array.Select(e => new Candidate<TEntity, TKey>(e, _jsonParser.Parse(e) as JsonObject));
                default:
                    return new List<ICandidate<TEntity, TKey>> { new Candidate<TEntity, TKey>(token, _jsonParser.Parse(token) as JsonObject) };
            }
        }

        protected virtual async Task<string> GetContentAsync(HttpRequest request)
        {
            using (var reader = new StreamReader(request.Body, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}