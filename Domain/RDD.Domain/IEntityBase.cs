using RDD.Domain.Models;
using System.Collections.Generic;

namespace RDD.Domain
{
	public interface IEntityBase : IPrimaryKey, IIncludable
	{
		string Name { get; }
		string Url { get; }
		ICollection<Operation> AuthorizedOperations { get; set; }
		Dictionary<string, bool> AuthorizedActions { get; set; }
	}

	public interface IEntityBase<TKey> : IEntityBase, IPrimaryKey<TKey> { }

	public interface IEntityBase<TEntity, TKey> : IEntityBase<TKey>, ICloneable<TEntity> { }
}
