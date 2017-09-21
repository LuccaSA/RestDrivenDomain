using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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

		IQueryable<TEntity> ApplyRights<TEntity>(IQueryable<TEntity> entities, HashSet<int> operations);
	}
}
