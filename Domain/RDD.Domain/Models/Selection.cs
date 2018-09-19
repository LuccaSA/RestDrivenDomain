using System.Collections.Generic;

namespace RDD.Domain.Models
{
    public class Selection<TEntity> : ISelection<TEntity>
        where TEntity : class
    {
        public IEnumerable<TEntity> Items { get; }
        public int Count { get; }

        public Selection(IEnumerable<TEntity> items, int count)
        {
            Items = items;
            Count = count;
        }

        IEnumerable<object> ISelection.GetItems() => Items;
    }
}