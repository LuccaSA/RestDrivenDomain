using RDD.Domain.Exceptions;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NExtends.Primitives.Types;
using System.Collections;
using LinqKit;
using RDD.Domain.Helpers;

namespace RDD.Domain.Models
{
	public partial class ReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
		where TEntity : class, IEntityBase<TEntity, TKey>
		where TKey : IEquatable<TKey>
	{
		public virtual IQueryable<TEntity> OrderByDefault(IQueryable<TEntity> entities)
		{
			return entities.OrderBy(e => e.Id);
		}
		
	}
}