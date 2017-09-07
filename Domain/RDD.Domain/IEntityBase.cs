using RDD.Domain.Contracts;
using RDD.Domain.Models;
using System.Collections.Generic;

namespace RDD.Domain
{
	public interface IEntityBase : IPrimaryKey, IIncludable, IReadableName
	{
		string Url { get; set; }
		ICollection<Operation> AuthorizedOperations { get; set; }
		Dictionary<string, bool> AuthorizedActions { get; set; }
	}

	public interface IEntityBase<TKey> : IEntityBase, IPrimaryKey<TKey> { }
}