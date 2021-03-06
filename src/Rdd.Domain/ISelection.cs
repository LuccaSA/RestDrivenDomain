﻿using System.Collections.Generic;

namespace Rdd.Domain
{
    public interface ISelection
    {
        int Count { get; }
        IEnumerable<object> GetItems();
    }

    public interface ISelection<out TEntity> : ISelection
        where TEntity : class
    {
        IEnumerable<TEntity> Items { get; }
    }
}