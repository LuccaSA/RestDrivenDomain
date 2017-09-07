using RDD.Domain.WebServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IWebServicesCollection : IRestCollection<WebService, int>
	{
		Task<IEnumerable<WebService>> GetByTokenAsync(string token);
	}
}
