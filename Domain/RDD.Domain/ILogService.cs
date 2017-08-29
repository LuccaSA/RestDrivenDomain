using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface ILogService
	{
		void Log(LogLevel level, string message);
		void Log(LogLevel level, string message, params object[] args);
	}

	public enum LogLevel { DEBUG = 0, INFO, WARNING, ERROR, CRITICAL };
}
