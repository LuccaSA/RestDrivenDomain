using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RDD.Domain;
using RDD.Domain.Helpers;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Web.Models
{
    public class Candidate<TEntity> : ICandidate<TEntity>
        where TEntity : class
    {
        private readonly JToken _structure;
        public TEntity Value { get; private set; }

        public Candidate(string json)
        {
            _structure = JToken.Parse(json);
            Value = JsonConvert.DeserializeObject<TEntity>(json);
        }

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

                            return ContainsPath(matchingChild.Value, selector.Children.ElementAt(0));
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

        public bool HasProperty(Expression<Func<TEntity, object>> expression)
        {
            var propertySelector = new PropertySelector<TEntity>(expression);

            return ContainsPath(_structure, propertySelector.Children.ElementAt(0));
        }
    }
}
