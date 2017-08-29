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

		bool HasOperation(IStorageService context, int operation);
		bool HasAnyOperations(IStorageService context, HashSet<int> operations);

		HashSet<int> GetOperations(IStorageService context, HashSet<int> operations);

		IQueryable<TEntity> ApplyRights<TEntity>(IStorageService context, IQueryable<TEntity> entities, HashSet<int> operations);
	}
}
