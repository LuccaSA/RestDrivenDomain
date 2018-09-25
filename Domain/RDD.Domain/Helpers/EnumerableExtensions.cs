using System.Collections.Generic;

namespace RDD.Domain.Helpers
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}
