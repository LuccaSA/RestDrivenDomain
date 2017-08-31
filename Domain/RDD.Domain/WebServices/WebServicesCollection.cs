using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;

namespace RDD.Domain.WebServices
{
	public class WebServicesCollection : RestCollection<WebService, int>, IWebServicesCollection
	{
		public WebServicesCollection(IStorageService storage, IExecutionContext execution, ICombinationsHolder combinationsHolder, Func<IStorageService> asyncStorage = null)
			: base(storage, execution, combinationsHolder, asyncStorage) { }

		public IEnumerable<WebService> GetByToken(string token)
		{
			return Get(new Query<WebService>
			{
				ExpressionFilters = ws => ws.Token == token
			}).Items;
		}
	}
}
