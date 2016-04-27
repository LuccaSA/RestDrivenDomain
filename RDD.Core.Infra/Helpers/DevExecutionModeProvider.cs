using RDD.Domain;
using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Helpers
{
	public class DevExecutionModeProvider : IExecutionModeProvider
	{
		public ExecutionMode GetExecutionMode() { return ExecutionMode.Dev; }
	}
}
