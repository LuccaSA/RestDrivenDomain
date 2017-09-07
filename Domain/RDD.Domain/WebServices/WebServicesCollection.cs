using RDD.Domain.Collections;
using RDD.Domain.Contracts;
using RDD.Domain.Models.Collections;
using RDD.Domain.Models.Convertors;
using RDD.Domain.Models.StorageQueries;
using System.Collections.Generic;
using System.Diagnostics;

namespace RDD.Domain.WebServices
{
	public class WebServicesCollection : RestCollection<WebService, int>, IWebServicesCollection
	{
		public WebServicesCollection(Stopwatch queryWatch, IRepository<WebService> repository, IQueryConvertor<WebService> convertor)
			: base(queryWatch, repository, convertor) { }

		public IEnumerable<WebService> GetByToken(string token)
		{
			return _repository.Get(StorageQuery<WebService>.Simple(_queryWatch, ws => ws.Token == token));
		}
	}
}