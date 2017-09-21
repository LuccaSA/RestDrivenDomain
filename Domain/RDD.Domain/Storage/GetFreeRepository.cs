﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDD.Domain.Models.Querying;

namespace RDD.Domain.Storage
{
	public class GetFreeRepository<TEntity> : Repository<TEntity>
		where TEntity : class, IEntityBase
	{
		public GetFreeRepository(IStorageService storageService, IExecutionContext executionContext, ICombinationsHolder combinationsHolder)
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