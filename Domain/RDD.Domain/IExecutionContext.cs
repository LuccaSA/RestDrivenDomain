using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IExecutionContext
	{
		IPrincipal curPrincipal { get; set; }
	}
}
