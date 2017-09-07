using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using RDD.Domain.Models.Querying;

namespace RDD.Web.Helpers
{
	public interface IApiHelper<TEntity>
	{
		Query<TEntity> CreateQuery(bool isCollectionCall = true);
		void IgnoreFilters(params string[] filters);
		List<PostedData> InputObjectsFromIncomingHTTPRequest(HttpContext context);
	}
}