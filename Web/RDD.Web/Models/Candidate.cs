﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Json;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Rdd.Web.Models
{
    public class Candidate<TEntity, TKey> : ICandidate<TEntity, TKey>
        where TEntity : IEntityBase<TKey>
    {
        private readonly JToken _structure;
        public TEntity Value { get; private set; }
        public JsonObject JsonValue { get; private set; }

        public Candidate(string json, JsonObject jsonValue)
        {
            _structure = JToken.Parse(json);
            Value = JsonConvert.DeserializeObject<TEntity>(json);
            JsonValue = jsonValue;
        }
        public static Candidate<TEntity, TKey> Parse(string json)
        {
            return new Candidate<TEntity, TKey>(json, new JsonParser().Parse(json) as JsonObject);
        }

        public bool HasId() => HasProperty(c => c.Id);
        public bool HasProperty<TProp>(Expression<Func<TEntity, TProp>> expression)
        {
            var selector = ExpressionChain<TEntity>.New(expression);
            return ContainsPath(_structure, selector);
        }
        TKey ICandidate<TEntity, TKey>.Id => Value.Id;
        object ICandidate<TEntity>.Id => Value.Id;

        private bool ContainsPath(JToken token, IExpressionChain selector)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    {
                        var matchingChild = token.Children<JProperty>().FirstOrDefault(c => string.Equals(c.Name, selector.Current.Name, StringComparison.InvariantCultureIgnoreCase));

                        if (matchingChild != null)
                        {
                            return !selector.HasNext() || ContainsPath(matchingChild.Value, selector.Next);
                        }

                        return false;
                    }

                case JTokenType.Array:
                    {
                        return ((JArray)token).All(child => ContainsPath(child, selector));
                    }

                default:
                    return true;
            }
        }
    }
}
