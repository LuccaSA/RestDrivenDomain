using RDD.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Net
{
	public class WrappedWebClient : System.Net.WebClient, IWebClient
	{
	}
}
