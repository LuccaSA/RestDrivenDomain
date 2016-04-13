using NUnit.Framework;
using RDD.Domain.Contexts;
using RDD.Domain.Exceptions;
using RDD.Infra.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Tests
{
	public class HttpLikeExceptionsTests
	{
		public HttpLikeExceptionsTests()
		{
			RDD.Infra.BootStrappers.TestsBootStrapper.ApplicationStart();
			RDD.Infra.BootStrappers.TestsBootStrapper.ApplicationBeginRequest();
			Resolver.Current().Register<ILogService>(() => new LostLogService());
		}

		[Test]
		public void ExceptionWithStatusAndMessageShouldWork()
		{
			var exception = new HttpLikeException(HttpStatusCode.Conflict, "My message");
		}
	}
}
