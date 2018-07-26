using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RDD.Domain;
using RDD.Domain.Helpers;
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
        public bool HasProperty(Expression<Func<TEntity, object>> expression)
        {
            var propertySelector = new PropertySelector<TEntity>(expression);

            return ContainsPath(_structure, propertySelector);
        }
        TKey ICandidate<TEntity, TKey>.Id => Value.Id;
        object ICandidate<TEntity>.Id => Value.Id;

        private bool ContainsPath(JToken token, PropertySelector selector)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    {
                        var matchingChild = token.Children<JProperty>().FirstOrDefault(c => String.Equals(c.Name, selector.Name, StringComparison.InvariantCultureIgnoreCase));

                        if (matchingChild != null)
                        {
                            if (!selector.HasChild)
                            {
                                return true;
                            }

                            return ContainsPath(matchingChild.Value, selector.Child);
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
