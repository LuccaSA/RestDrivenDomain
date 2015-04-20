using RDD.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Samples.Infra
{
	public class ExecutionContext : IExecutionContext
	{
		IPrincipal _curPrincipal;
		public IPrincipal curPrincipal
		{
			get { return _curPrincipal; }
			set
			{
				_curPrincipal = value;
			}
		}
		public Stopwatch serverWatch { get; private set; }
		public Stopwatch queryWatch { get; private set; }

		public ExecutionContext()
		{
			serverWatch = new Stopwatch();
			serverWatch.Start();

			queryWatch = new Stopwatch();
		}

		public void Authentify()
		{
		}

		public void Dispose() { }
	}
}