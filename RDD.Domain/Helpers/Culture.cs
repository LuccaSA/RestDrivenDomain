using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Helpers
{
	public class Culture
	{
		public CultureInfo rawCulture { get; private set; }

		public int Id { get { return rawCulture.LCID; } set { throw new NotImplementedException(); } }

		public string Code { get { return rawCulture.Name; } }

		public string Name { get { return rawCulture.NativeName; } set { throw new NotImplementedException(); } }

		public string EnglishName { get { return rawCulture.EnglishName; } }

		public int LCID { get { return rawCulture.LCID; } }

		public Culture() { }

		public Culture(CultureInfo rawCulture)
		{
			this.rawCulture = rawCulture;
		}
	}
}
