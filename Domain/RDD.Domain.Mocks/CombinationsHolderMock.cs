using RDD.Domain.Models;
using RDD.Domain.Rights;
using System.Collections.Generic;

namespace RDD.Domain.Mocks
{
    public class CombinationsHolderMock : ICombinationsHolder
    {
        public IEnumerable<Combination> Combinations => new List<Combination>();
    }
}
