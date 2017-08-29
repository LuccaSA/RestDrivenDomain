using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Exceptions
{
	public class NotFoundException : HttpLikeException
	{
		public NotFoundException()
			: base(HttpStatusCode.NotFound) { }

		public NotFoundException(string message)
			: base(HttpStatusCode.NotFound, message) { }
	}
}
