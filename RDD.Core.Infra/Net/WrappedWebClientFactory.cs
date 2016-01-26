using RDD.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Net
{
	// http://brunov.info/blog/2013/07/30/tdd-mocking-system-net-webclient/
	public class WrappedWebClientFactory : IWebClientFactory
	{
		public IWebClient Create()
		{
			return new WrappedWebClient();
		}
	}
}
