using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDD.Domain
{
    public interface IPrincipal
    {
        int Id { get; }
        string Token { get; set; }
        string Name { get; }
        Culture Culture { get; }

        bool HasOperation(int operation);
        bool HasAnyOperations(HashSet<int> operations);

        HashSet<int> GetOperations(HashSet<int> operations);

        Expression<Func<TEntity, bool>> ApplyRights<TEntity>(HashSet<int> operations);
    }
}
