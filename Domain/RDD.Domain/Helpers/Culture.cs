using System;
using System.Globalization;

namespace RDD.Domain.Helpers
{
	public class Culture
	{
		public Culture() { }

		public Culture(CultureInfo rawCulture)
		{
			RawCulture = rawCulture;
		}

		public CultureInfo RawCulture { get; }

		public int Id { get => RawCulture.LCID;
		    set => throw new NotImplementedException();
		}

		public string Code => RawCulture.Name;

	    public string Name { get => RawCulture.NativeName;
	        set => throw new NotImplementedException();
	    }

		public string EnglishName => RawCulture.EnglishName;

	    public int LCID => RawCulture.LCID;
	}
}
