﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Helpers
{
	public class Culture
	{
		public CultureInfo RawCulture { get; private set; }

		public int Id { get { return RawCulture.LCID; } set { throw new NotImplementedException(); } }

		public string Code { get { return RawCulture.Name; } }

		public string Name { get { return RawCulture.NativeName; } set { throw new NotImplementedException(); } }

		public string EnglishName { get { return RawCulture.EnglishName; } }

		public int LCID { get { return RawCulture.LCID; } }

		public Culture() { }

		public Culture(CultureInfo rawCulture)
		{
			this.RawCulture = rawCulture;
		}
	}
}