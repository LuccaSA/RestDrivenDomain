using RDD.Domain.Helpers;
using System.Collections.Generic;
using System.Reflection;

namespace RDD.Domain
{
    public interface ISelection<TEntity> : ISelection
        where TEntity : class, IEntityBase
    {
        IEnumerable<TEntity> Items { get; }
    }

    public interface ISelection
    {
        int Count { get; }
        object Sum(PropertyInfo property, DecimalRounding rouding);
        object Min(PropertyInfo property, DecimalRounding rouding);
        object Max(PropertyInfo property, DecimalRounding rouding);
    }
}
