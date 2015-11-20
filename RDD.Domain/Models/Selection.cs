using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Models
{
	public class Selection<TEntity> : ISelection<TEntity>
		where TEntity : class, IEntityBase
	{
		public IEnumerable<TEntity> Items { get; private set; }
		public int Count { get; private set; }

		public Selection(IEnumerable<TEntity> items, int count)
		{
			Items = items;
			Count = count;
		}
	}
}
