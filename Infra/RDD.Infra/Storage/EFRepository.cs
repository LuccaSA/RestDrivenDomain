using RDD.Domain;
using RDD.Domain.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using RDD.Domain.Models.Querying;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RDD.Infra.Storage
{
    public class EFRepository<TEntity> : Repository<TEntity>
		where TEntity : class, IEntityBase
	{
		public EFRepository(IStorageService storageService, IExecutionContext executionContext, ICombinationsHolder combinationsHolder)
			: base(storageService, executionContext, combinationsHolder) { }

		public override async Task<int> CountAsync(Query<TEntity> query)
		{
			return await QueryEntities(query).CountAsync();
		}

		public override async Task<IEnumerable<TEntity>> EnumerateAsync(Query<TEntity> query)
		{
			return await QueryEntities(query).ToListAsync();
		}
	}
}
