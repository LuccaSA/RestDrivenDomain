using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RDD.Domain
{
	public enum AuthenticationSource
	{
		Web = 0,
		Api
	}

	public interface IAuthenticationService
	{
		IPrincipal Authenticate();
		IPrincipal Authenticate(AuthenticationSource source);
//		IPrincipal GetPrincipalByToken(string authToken);
		void HandleUnauthorizedRequest();
	}
}
