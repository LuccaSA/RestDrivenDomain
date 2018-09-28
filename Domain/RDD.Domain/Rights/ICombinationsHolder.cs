using RDD.Domain.Models;
using System.Collections.Generic;

namespace RDD.Domain.Rights
{
    public interface ICombinationsHolder
    {
        IEnumerable<Combination> Combinations { get; }
    }
}
