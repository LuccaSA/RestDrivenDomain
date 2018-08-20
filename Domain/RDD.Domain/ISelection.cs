using RDD.Domain.Helpers;
using System.Collections.Generic;
using System.Reflection;

namespace RDD.Domain
{
    public interface ISelection
    {
        IEnumerable<object> Items { get; }
        int Count { get; }
        object Sum(PropertyInfo property, DecimalRounding rouding);
        object Min(PropertyInfo property, DecimalRounding rouding);
        object Max(PropertyInfo property, DecimalRounding rouding);
    }
    public interface ISelection<TEntity> : ISelection
        where TEntity : class
    {
        new IEnumerable<TEntity> Items { get; }
    }
}
