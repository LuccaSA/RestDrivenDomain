using RDD.Domain.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using RDD.Domain.Models.Querying;
using System.Linq;

namespace RDD.Domain.Tests.Models
{
	public class OpenRepository<TEntity> : Repository<TEntity>
		where TEntity : class, IEntityBase
	{
		public OpenRepository(IStorageService storageService, IExecutionContext executionContext, ICombinationsHolder combinationsHolder)
		: base(storageService, executionContext, combinationsHolder) { }

		protected override IQueryable<TEntity> ApplyRights(IQueryable<TEntity> entities, Query<TEntity> query)
		{
			if (query.Verb == Helpers.HttpVerb.GET)
			{
				return entities;
			}

			return base.ApplyRights(entities, query);
		}
	}
}