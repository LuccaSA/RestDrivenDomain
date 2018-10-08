using Rdd.Domain.Models;
using Rdd.Domain.Rights;
using System.Collections.Generic;

namespace Rdd.Domain.Mocks
{
    public class CombinationsHolderMock : ICombinationsHolder
    {
        public IEnumerable<Combination> Combinations => new List<Combination>();
    }
}
