using RDD.Infra.Models.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Localization
{
	public class Culture : EntityBase<Culture, int>
	{
		private CultureInfo _rawCulture;

		public override int Id { get { return _rawCulture.LCID; } set { throw new NotImplementedException(); } }

		public string Code { get { return _rawCulture.Name; } }

		public override string Name { get { return _rawCulture.NativeName; } set { throw new NotImplementedException(); } }

		public string EnglishName { get { return _rawCulture.EnglishName; } }

		public int LCID { get { return _rawCulture.LCID; } }

		public Culture() { throw new NotImplementedException(); }
		public Culture(CultureInfo rawCulture)
		{
			_rawCulture = rawCulture;
		}
	}
}
