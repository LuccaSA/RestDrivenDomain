using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public interface IPrimaryKey<TKey>
		where TKey : IEquatable<TKey>
	{
		TKey Id { get; }
	}
}
