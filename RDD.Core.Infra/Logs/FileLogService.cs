using RDD.Domain;
using RDD.Domain.Contexts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Logs
{
	public class FileLogService : ILogService
	{
		/// <summary>
		/// Locker to avoid writing to the log file at the same time
		/// </summary>
		private static object _locker = new object();

		/// <summary>
		/// Log level to use for application
		/// </summary>
		private LogLevel _minLevel { get; set; }

		/// <summary>
		/// Folder to save log files to
		/// </summary>
		private string _folder { get; set; }

		/// <summary>
		/// Log files will have the following name: [prefix]-[year]-[week].txt
		/// </summary>
		private string _fileNamePrefix { get; set; }

		public FileLogService(string folder, string fileNamePrefix)
			: this(LogLevel.DEBUG, folder, fileNamePrefix) { }

		public FileLogService(LogLevel minLevel, string folder, string fileNamePrefix)
		{
			_minLevel = minLevel;
			_folder = folder;
			_fileNamePrefix = fileNamePrefix;
		}

		public static LogLevel ParseLevel(string level)
		{
			return (LogLevel)Enum.Parse(typeof(LogLevel), level.ToUpper(), true);
		}

		public void Log(LogLevel level, string message)
		{
			// If we wish to log messages above a given level
			// eg: anything above WARNING
			// if (level <= settings.LogLevel)
			try
			{
				lock (_locker)
				{
					if (level >= _minLevel)
					{
						var now = DateTime.Now;
						var culture = CultureInfo.CurrentCulture;
						var weekNum = culture.Calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

						var fileName = String.Format("{0}-{1}-w{2}.txt", this._fileNamePrefix, DateTime.Now.ToString("yyyy"), weekNum);
						var filePath = Path.Combine(_folder, fileName);

						var text = String.Format("{0}\t{1}\t{2}\r\n", DateTime.Now.ToString("s"), level.ToString(), message);

						File.AppendAllText(filePath, text);

						if (level == LogLevel.CRITICAL)
						{
							Resolver.Current().Resolve<IMailService>().SendCriticalExceptionMail(message);
						}
					}
				}
			}
			catch { }
		}

		public void Log(LogLevel level, string message, params object[] args)
		{
			Log(level, String.Format(message, args));
		}
	}
}
