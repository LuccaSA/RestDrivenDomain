using RDD.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Logs
{
	public class LostLogService : ILogService
	{
		public void Log(LogLevel level, string message) { }
		public void Log(LogLevel level, string message, params object[] args) { }
	}
}
