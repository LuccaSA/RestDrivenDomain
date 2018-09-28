using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Rights;
using System.Collections.Generic;

namespace RDD.Web.Tests.ServerMock
{
    public class CombinationsHolder : ICombinationsHolder
    {
        public CombinationsHolder()
        {
            Combinations = new[]
            {
                new Combination
                {
                    Operation = new Operation(),
                    Subject = typeof(ExchangeRate),
                    Verb = HttpVerb.All
                }
            };
        }

        public IEnumerable<Combination> Combinations { get; }
    }
}