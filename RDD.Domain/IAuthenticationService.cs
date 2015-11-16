using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RDD.Domain
{
	public interface IAuthenticationService
	{
		IPrincipal Authenticate();
		IPrincipal GetPrincipalByToken(string authToken);
	}
}
