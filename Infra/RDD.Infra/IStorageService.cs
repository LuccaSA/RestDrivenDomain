﻿using RDD.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Infra
{
    public interface IStorageService : IDisposable
    {
        IQueryable<TEntity> Set<TEntity>() where TEntity : class, IEntityBase;
        Task<IEnumerable<TEntity>> EnumerateEntitiesAsync<TEntity>(IQueryable<TEntity> entities) where TEntity : class, IEntityBase;
        void Add<TEntity>(TEntity entity) where TEntity : class, IEntityBase;
        void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntityBase;
        void Remove<TEntity>(TEntity entity) where TEntity : class, IEntityBase;
        Task SaveChangesAsync();
        void AddAfterSaveChangesAction(Task action);
    }
}