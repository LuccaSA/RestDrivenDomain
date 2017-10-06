using RDD.Domain;
using RDD.Domain.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using RDD.Domain.Models.Querying;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
