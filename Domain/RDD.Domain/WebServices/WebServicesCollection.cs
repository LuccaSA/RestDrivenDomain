using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.WebServices
{
	public class WebServicesCollection : RestCollection<WebService, int>, IWebServicesCollection
	{
		public WebServicesCollection(IStorageService storage, IExecutionContext execution, Func<IStorageService> asyncStorage = null)
			: base(storage, execution, asyncStorage) { }

		public IEnumerable<WebService> GetByToken(string token)
		{
			return Get(new Query<WebService>
			{
				ExpressionFilters = ws => ws.Token == token
			}).Items;
		}
	}
}
