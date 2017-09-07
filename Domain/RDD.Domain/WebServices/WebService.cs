﻿using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.WebServices
{
	public class WebService : EntityBase<WebService, int>, IPrincipal
	{
		public override int Id { get; set; }
		public override string Name { get; set; }
		public string Token { get; set; }

		public Culture Culture
		{
			get { return new Culture(CultureInfo.GetCultureInfo("en-US")); }
		}

		public ICollection<int> AppOperations { get; set; }

		public WebService()
		{
			AppOperations = new HashSet<int>();
		}

		public virtual HashSet<int> GetOperations(HashSet<int> operations)
		{
			return new HashSet<int>(AppOperations.Intersect(operations));
		}

		public virtual bool HasAnyOperations(HashSet<int> operations)
		{
			return GetOperations(operations).Any();
		}

		public virtual bool HasOperation(int operation)
		{
			return GetOperations(new HashSet<int>() { operation }).Any();
		}

		public virtual IQueryable<TEntity> ApplyRights<TEntity>(IQueryable<TEntity> entities, HashSet<int> operations)
		{
			if (!HasAnyOperations(operations))
			{
				throw new HttpLikeException(HttpStatusCode.Unauthorized, String.Format("Web service {0} does not have any permission on type {1}", Name, typeof(TEntity).Name));
			}

			return entities;
		}
	}
}
