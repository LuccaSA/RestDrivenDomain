using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Web.Authentication
{
	public class HttpAuthenticationService : IAuthenticationService
	{
		private IWebContext _webContext;
		private IWebServicesCollection _webServices;

		public HttpAuthenticationService(IWebContext webContext, IWebServicesCollection webServices)
		{
			_webContext = webContext;
			_webServices = webServices;
		}

		public IPrincipal Authenticate()
		{
			return Authenticate(AuthenticationSource.Api);
		}

		public IPrincipal Authenticate(AuthenticationSource source)
		{
			switch (source)
			{
				case AuthenticationSource.Api:
					{
						var token = RetrieveTokenFromAuthorizationHeader("application");

						if (token.HasValue)
						{
							var webService = _webServices.GetByToken(token.Value.ToString()).FirstOrDefault();

							if (webService != null)
							{
								return webService;
							}
						}

						HandleUnauthorizedRequest();
						break;
					}
			}

			throw new NotImplementedException();
		}

		/// <summary>
		/// Retrieve GUID from Authorization header
		/// </summary>
		/// <param name="authType">"application" for an appToken and "user" for an auth/long token</param>
		/// <returns></returns>
		private Guid? RetrieveTokenFromAuthorizationHeader(string authType)
		{
			if (_webContext.Headers.AllKeys.Contains("Authorization"))
			{
				//On récupère la méthode d'authorization utilisée => Basic token ou Lucca user=token ou Lucca application=token
				var autorisationParts = _webContext.Headers["Authorization"].Split(' ');
				if (autorisationParts[0].ToLower() == "lucca")
				{
					var applicationParts = autorisationParts[1].Split('=');
					if (applicationParts[0] == authType)
					{
						Guid result;
						Guid.TryParse(applicationParts[1], out result);

						return result;
					}
				}
			}

			return null;
		}

		public void HandleUnauthorizedRequest()
		{
			throw new UnauthorizedException("Your token is invalid or you did not provide any");
		}
	}
}
