using Microsoft.Extensions.DependencyInjection;
using RDD.Domain;
using RDD.Domain.Contexts;
using RDD.Infra.Contexts;
using RDD.Infra.DependencyInjection;
using RDD.Infra.Helpers;
using RDD.Infra.Services;

namespace RDD.Infra.BootStrappers
{
	public static class TestsBootStrapper
	{
		public static void ApplicationStart(IServiceCollection services)
		{
			services.AddSingleton<IAsyncService, AsyncService>();
			services.AddSingleton<IExecutionModeProvider, TestExecutionModeProvider>();
		}
	}
}
