using RDD.Domain;
using RDD.Domain.Contexts;
using RDD.Domain.WebServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RDD.Web.Authentication
{
	public class ApiAuthorizeAttribute : AuthorizeAttribute
	{
		protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
		{
			Resolver.Current().Resolve<IAuthenticationService>().HandleUnauthorizedRequest();
		}

		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			return Resolver.Current().Resolve<IAuthenticationService>().Authenticate() != null;
		}

		public override void OnAuthorization(HttpActionContext actionContext)
		{
			var resolver = Resolver.Current();
			var principal = resolver.Resolve<IAuthenticationService>().Authenticate();

			if (principal == null)
			{
				HandleUnauthorizedRequest(actionContext);
			}

			resolver.Resolve<IExecutionContext>().curPrincipal = principal;
		}
	}
}
