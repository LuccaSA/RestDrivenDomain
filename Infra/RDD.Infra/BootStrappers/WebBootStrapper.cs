using Microsoft.Extensions.DependencyInjection;
using RDD.Domain;
using RDD.Infra.Helpers;
using RDD.Infra.Services;

namespace RDD.Infra.BootStrappers
{
	public static class WebBootStrapper
	{
		public static void ApplicationStart(IServiceCollection services)
		{
			services.AddSingleton<IAsyncService, AsyncService>();
			services.AddSingleton<IExecutionModeProvider, DevExecutionModeProvider>();
		}
	}
}
