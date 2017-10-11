using RDD.Domain.Models;
using System.Collections.Generic;

namespace RDD.Domain.Mocks
{
    public class CombinationsHolderMock : ICombinationsHolder
    {
        public IEnumerable<Combination> Combinations => new List<Combination>();
    }
}
