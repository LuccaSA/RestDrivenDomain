using Microsoft.EntityFrameworkCore.Query;
using System.Linq;

namespace RDD.Domain.Models.StorageQueries.Includers
{
	public interface IMonoIncluder<T> : IIncluder<T>
	{
		IQueryable<TInitial> ApplyInclude<TInitial>(IIncludableQueryable<TInitial, T> query) where TInitial : class;
	}
}