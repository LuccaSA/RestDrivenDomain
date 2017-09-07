using System.Linq;

namespace RDD.Infra.Repositories.Mappings
{
	public interface IQueryableConvertor<TInput, TOutput> 
	{
		IQueryable<TOutput> Convert(IQueryable<TInput> request);
	}
}