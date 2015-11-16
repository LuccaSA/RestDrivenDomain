using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IEntityBase : IPrimaryKey, IIncludable
	{
		string Name { get; set; }
		string Url { get; set; }
		ICollection<Operation> Operations { get; set; }
		Dictionary<string, bool> Actions { get; set; }

		void Forge(IStorageService storage, Options queryOptions);
	}

	public interface IEntityBase<TKey> : IEntityBase, IPrimaryKey<TKey> { }

	public interface IEntityBase<TEntity, TKey> : IEntityBase<TKey>, ICloneable<TEntity>
	{
		void Validate(IStorageService storage, TEntity oldEntity);
	}
}
