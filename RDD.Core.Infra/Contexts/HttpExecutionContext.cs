using RDD.Domain;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RDD.Infra.Contexts
{
	public class HttpExecutionContext : IExecutionContext
	{
		IPrincipal _curPrincipal;
		public IPrincipal curPrincipal
		{
			get { return _curPrincipal; }
			set
			{
				_curPrincipal = value;
				if (_curPrincipal != null && _curPrincipal.Culture != null && _curPrincipal.Culture.RawCulture != null)
				{
					Thread.CurrentThread.CurrentCulture = _curPrincipal.Culture.RawCulture;
					Thread.CurrentThread.CurrentUICulture = _curPrincipal.Culture.RawCulture;
				}
			}
		}
		public Stopwatch serverWatch { get; private set; }
		public Stopwatch queryWatch { get; private set; }

		public HttpExecutionContext()
		{
			serverWatch = new Stopwatch();
			serverWatch.Start();

			queryWatch = new Stopwatch();
		}
	}
}
