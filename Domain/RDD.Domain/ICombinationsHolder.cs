using RDD.Domain.Models;
using System.Collections.Generic;

namespace RDD.Domain
{
	public interface ICombinationsHolder
	{
		IEnumerable<Combination> Combinations { get; }
	}
}
