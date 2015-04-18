using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Enums
{
	[Flags]
	public enum HttpVerb : byte
	{
		GET = 1,
		POST = 2,
		PUT = 4,
		DELETE = 8,

		ALL = GET | POST | PUT | DELETE,
		ALL_NO_DELETE = GET | POST | PUT
	};
}
