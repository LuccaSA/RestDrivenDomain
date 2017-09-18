using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IRepository<TEntity>
		where TEntity : class, IEntityBase
	{
		Task<int> CountAsync(Query<TEntity> query);
		Task<IEnumerable<TEntity>> EnumerateAsync(Query<TEntity> query);
		void Add(TEntity entity);
		void Remove(TEntity entity);
	}
}
