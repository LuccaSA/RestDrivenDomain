using Microsoft.AspNetCore.Http;
using RDD.Domain;
using RDD.Infra.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RDD.Infra.Contexts
{
	public class WebContextProvider : IWebContextProvider
	{
		public IWebContext GetContext()
		{
			var context = HttpContext.Current;

			if (context != null)
			{
				return new HttpContextWrapper(context);
			}
			else
			{
				return AsyncService.ThreadedContexts[Thread.CurrentThread.ManagedThreadId];
			}
		}
	}
}
