using Microsoft.EntityFrameworkCore;
using RDD.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Infra.Storage
{
	public class EFRepository<TEntity> : Repository<TEntity>
		where TEntity : class, IEntityBase
	{
		public EFRepository(IStorageService storageService, IExecutionContext executionContext, ICombinationsHolder combinationsHolder)
			: base(storageService, executionContext, combinationsHolder) { }

		protected override async Task<int> CountEntities(IQueryable<TEntity> entities)
		{
			return await entities.CountAsync();
		}

		protected override async Task<IEnumerable<TEntity>> EnumerateEntities(IQueryable<TEntity> entities)
		{
			return await entities.ToListAsync();
		}
	}
}
