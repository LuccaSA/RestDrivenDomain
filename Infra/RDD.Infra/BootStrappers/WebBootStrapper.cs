using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RDD.Domain;
using RDD.Domain.Contexts;
using RDD.Infra.Contexts;
using RDD.Infra.DependencyInjection;
using RDD.Infra.Helpers;
using RDD.Infra.Logs;
using RDD.Infra.Services;
using System;
using System.Threading;
using HttpContextWrapper = RDD.Infra.Contexts.HttpContextWrapper;

namespace RDD.Infra.BootStrappers
{
	public static class WebBootStrapper
	{
		public static void ApplicationStart(IServiceCollection services)
		{
			services.AddSingleton<IAsyncService, AsyncService>();
			services.AddSingleton<ILogService, LostLogService>();
			services.AddSingleton<IExecutionModeProvider, DevExecutionModeProvider>();
		}
	}
}
