using RDD.Domain.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NExtends.Primitives;

namespace RDD.Domain.Models
{
	public class Enum<TEnum>
		where TEnum : struct
	{
		public Enum(TEnum value)
		{
			var type = typeof(TEnum);

			if (!type.IsEnum)
			{
				throw new ArgumentException(String.Format("{0} is not an enumerated type.", type.Name));
			}

			Value = value;
		}

		/// <summary>
		/// Enum to serialise
		/// </summary>
		internal TEnum Value { get; private set; }

		/// <summary>
		/// Enum "int" identifier (stored in DB)
		/// </summary>
		public int Id
		{
			get { return Convert.ToInt32(Value); }
			set { Value = (TEnum)((object)value); }
		}

		/// <summary>
		/// Enum "word" identifier
		/// </summary>
		public string Code
		{
			get { return Convert.ToString(Value); }
			set { Value = value.Parse<TEnum>(); }
		}

		/// <summary>
		/// Enum name
		/// </summary>
		public string Name
		{
			get
			{
				var memInfo = typeof(TEnum).GetMember(Value.ToString());
				var attribute = (CulturedDescriptionAttribute)memInfo[0].GetCustomAttributes(typeof(CulturedDescriptionAttribute), false).FirstOrDefault();

				return attribute == null ? Code : attribute.Description;
			}
		}
	}
}
