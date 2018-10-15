using System.Collections.Generic;

namespace Rdd.Domain.Helpers
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Returns a generic IEnumerable for a single item, avoiding useless List or Array allocation
        /// </summary>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}
