using RDD.Domain.Models.StorageQueries.Filters;
using RDD.Domain.Models.StorageQueries.Includers;
using RDD.Domain.Models.StorageQueries.Orderers;
using RDD.Domain.Models.StorageQueries.Pagers;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace RDD.Domain.Models.StorageQueries
{
	public class StorageQuery<T> : IStorageQuery<T> where T : class
	{
		public Stopwatch Watch { get; private set; }

		public IFilter<T> Filter { get; private set; }
		public IIncluder<T> Includer { get; private set; }
		public IOrderer<T> Orderer { get; private set; }
		public IPager Pager { get; private set; }

		public StorageQuery(Stopwatch watch, IFilter<T> filter, IIncluder<T> includer, IOrderer<T> orderer, IPager pager)
		{
			Watch = watch;

			Filter = filter;
			Orderer = orderer;
			Pager = pager;
			Includer = includer;
		}

		public static IStorageQuery<T> Simple(Stopwatch watch, Expression<Func<T, bool>> filter)
		{
			return new StorageQuery<T>(watch, new Filter<T>(filter), new EmptyIncluder<T>(), new EmptyOrderer<T>(), new EmptyPager());
		}
	}
}