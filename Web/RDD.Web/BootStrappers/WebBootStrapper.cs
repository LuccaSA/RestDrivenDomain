using Microsoft.Extensions.DependencyInjection;
using RDD.Domain;
using RDD.Infra.Helpers;
using RDD.Web.Contexts;

namespace RDD.Web.BootStrappers
{
	public static class WebBootStrapper
	{
		public static void ApplicationStart(IServiceCollection services)
		{
			services.AddSingleton<IAsyncService, AsyncService>();
			services.AddSingleton<IExecutionModeProvider, DevExecutionModeProvider>();
			services.AddScoped<IWebContext, HttpContextWrapper>();
		}
	}
}
