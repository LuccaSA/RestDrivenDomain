using Microsoft.AspNetCore.Http;

namespace RDD.Infra
{
	public interface IWebContextWrapper : IWebContext
	{
		void SetContext(HttpContext context);
	}
}
