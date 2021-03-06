﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rdd.Domain;
using Rdd.Domain.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rdd.Domain.Helpers;

namespace Rdd.Web.Querying
{
    public class CandidateParser : ICandidateParser
    {
        private readonly IJsonParser _jsonParser;
        private readonly JsonSerializer _serializer;

        public CandidateParser(IJsonParser jsonParser, IOptions<MvcNewtonsoftJsonOptions> jsonOptions)
        {
            _jsonParser = jsonParser ?? throw new ArgumentNullException(nameof(jsonParser));
            _serializer = JsonSerializer.Create(jsonOptions?.Value.SerializerSettings ?? throw new ArgumentNullException(nameof(jsonOptions)));
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
            return token switch
            {
                JArray array => array.Select(e => new Candidate<TEntity, TKey>(e, _jsonParser.Parse(e) as JsonObject, e.ToObject<TEntity>(_serializer))),
                _ => new Candidate<TEntity, TKey>(token, _jsonParser.Parse(token) as JsonObject, token.ToObject<TEntity>(_serializer)).Yield(),
            };
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