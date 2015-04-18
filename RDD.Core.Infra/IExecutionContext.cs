using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public interface IExecutionContext : IDisposable
	{
		IPrincipal curPrincipal { get; set; }
		Stopwatch serverWatch { get; }
		Stopwatch queryWatch { get; }
		//Func<IStorageService> NewStorageService { get; set; }
	}
}
