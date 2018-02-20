using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RDD.Domain.Models.Querying
{
    public class Candidate<TEntity>
        where TEntity : class
    {
        private JToken _structure;

        public Candidate(string json)
        {
            _structure = JToken.Parse(json);
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

        public bool HasValue(Expression<Func<TEntity, object>> expression)
        {
            var propertySelector = new PropertySelector<TEntity>(expression);

            return ContainsPath(_structure, propertySelector.Children.ElementAt(0));
        }
    }
}
