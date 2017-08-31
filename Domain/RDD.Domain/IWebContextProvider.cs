using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RDD.Domain
{
	public interface IWebContextProvider
	{
		IWebContext GetContext();
	}
}
