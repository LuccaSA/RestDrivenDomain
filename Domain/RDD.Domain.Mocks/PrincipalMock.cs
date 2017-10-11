using RDD.Domain.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Domain.Mocks
{
    public class PrincipalMock : IPrincipal
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string Name { get; set; }
        public Culture Culture { get; }

        public bool HasOperation(int operation) { return true; }
        public bool HasAnyOperations(HashSet<int> operations) { return true; }

        public HashSet<int> GetOperations(HashSet<int> operations) { return operations; }

        public IQueryable<TEntity> ApplyRights<TEntity>(IQueryable<TEntity> entities, HashSet<int> operations)
        {
            return entities;
        }
    }
}
