using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Helpers
{
	public enum ExecutionMode
	{
		Dev = 0,
		Test,
		Integration,
		ReleaseCandidate,
		Production
	}
}
