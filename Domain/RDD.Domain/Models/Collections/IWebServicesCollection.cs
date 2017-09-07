using RDD.Domain.Models.Collections;
using RDD.Domain.WebServices;
using System.Collections.Generic;

namespace RDD.Domain.Collections
{
	public interface IWebServicesCollection : IRestCollection<WebService, int>
	{
		IEnumerable<WebService> GetByToken(string token);
	}
}
