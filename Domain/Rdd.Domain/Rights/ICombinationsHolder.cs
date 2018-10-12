using Rdd.Domain.Models;
using System.Collections.Generic;

namespace Rdd.Domain.Rights
{
    public interface ICombinationsHolder
    {
        IEnumerable<Combination> Combinations { get; }
    }
}
