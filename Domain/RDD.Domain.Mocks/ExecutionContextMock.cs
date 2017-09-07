using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Mocks
{
	public class ExecutionContextMock : Mock<IExecutionContext>, IExecutionContext
	{
		public IPrincipal curPrincipal { get; set; }
		public Stopwatch serverWatch { get; private set; }
		public Stopwatch queryWatch { get; private set; }

		public ExecutionContextMock()
		{
			curPrincipal = new PrincipalMock();
			serverWatch = new Stopwatch();
			queryWatch = new Stopwatch();
		}
	}
}
