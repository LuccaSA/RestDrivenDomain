using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface ISelection<TEntity>
		where TEntity : class, IEntityBase
	{
		IEnumerable<TEntity> Items { get; }
		int Count { get; }
	}
}
