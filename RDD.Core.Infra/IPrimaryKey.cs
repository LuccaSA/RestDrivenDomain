using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public interface IPrimaryKey
	{
		object GetId();
	}

	public interface IPrimaryKey<TKey> : IPrimaryKey
		where TKey : IEquatable<TKey>
	{
		TKey Id { get; }
	}
}
