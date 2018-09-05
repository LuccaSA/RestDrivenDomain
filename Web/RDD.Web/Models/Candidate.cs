using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Helpers.Expressions;
using RDD.Domain.Json;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Web.Models
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
            var selector = ExpressionSelectorChain<TEntity>.New(expression);
            return ContainsPath(_structure, selector);
        }
        TKey ICandidate<TEntity, TKey>.Id => Value.Id;
        object ICandidate<TEntity>.Id => Value.Id;

        private bool ContainsPath(JToken token, IExpressionSelectorChain selector)
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
