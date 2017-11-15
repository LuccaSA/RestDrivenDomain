using System.Collections.Generic;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models;

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
                    Verb = HttpVerbs.All
                }
            };
        }

        public IEnumerable<Combination> Combinations { get; }
    }
}