using RDD.Domain.Models;
using System.Collections.Generic;

namespace RDD.Domain.Mocks
{
	public class CombinationsHolderMock : ICombinationsHolder
	{
		public IEnumerable<Combination> Combinations
		{
			get
			{
				return new List<Combination>();
			}
		}
	}
}
