using Rdd.Domain.Helpers;
using Rdd.Domain.Models;
using Rdd.Domain.Rights;
using System.Collections.Generic;

namespace Rdd.Web.Tests.ServerMock
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