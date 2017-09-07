using RDD.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
