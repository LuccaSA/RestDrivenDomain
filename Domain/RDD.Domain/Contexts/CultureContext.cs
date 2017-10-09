using System;
using System.Globalization;
using System.Threading;

namespace RDD.Domain.Contexts
{
	public class CultureContext : IDisposable
	{
		private readonly CultureInfo _originalCulture;
		private readonly CultureInfo _originalUICulture;

		public CultureContext(CultureInfo culture)
		{
			_originalCulture = Thread.CurrentThread.CurrentCulture;
			_originalUICulture = Thread.CurrentThread.CurrentUICulture;

			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;
		}

		public void Dispose()
		{
			Thread.CurrentThread.CurrentCulture = _originalCulture;
			Thread.CurrentThread.CurrentUICulture = _originalUICulture;
		}
	}
}
