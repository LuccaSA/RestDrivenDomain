using RDD.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RDD.Domain.Helpers
{
	public class DecimalRounding
	{
		public static DecimalRounding Default = new DecimalRounding(0, RoudingType.Floor);

		public enum RoudingType
		{
			//MidpointRounding.AwayFromZero strategy
			Round = 0,

			//MidpointRounding.ToEven strategy
			RoundEven,

			//Math.Ceiling
			Ceiling,

			//Math.Floor
			Floor
		}

		public int NumberOfDecimals { get; private set; }
		public RoudingType Type { get; private set; }

		public DecimalRounding(int numberOfDecimals, RoudingType type)
		{
			NumberOfDecimals = numberOfDecimals;
			Type = type;
		}

		public Func<double, double> GetRoundingFunction()
		{
			switch (Type)
			{
				case DecimalRounding.RoudingType.Round:
					return (d) => Math.Round(d, NumberOfDecimals, MidpointRounding.AwayFromZero);

				case DecimalRounding.RoudingType.RoundEven:
					return (d) => Math.Round(d, NumberOfDecimals, MidpointRounding.ToEven);

				case DecimalRounding.RoudingType.Ceiling:
					return (d) => Math.Ceiling(d);

				case DecimalRounding.RoudingType.Floor:
					return (d) => Math.Floor(d);

				default:
					throw new HttpLikeException(HttpStatusCode.BadRequest, string.Format("Unknown rounding strategy '{0}'", Type.ToString()));
			}
		}

		public static DecimalRounding Parse(string pattern)
		{
			var matches = Regex.Match(pattern, "sum\\(([a-zA-Z0-9_]*),?([a-zA-Z0-9_]*),?([a-zA-Z0-9_]*)\\)").Groups;
			var matchType = matches[2].Value;
			var matchDecimals = matches[3].Value;

			var rouding = DecimalRounding.Default;

			if (!String.IsNullOrEmpty(matchType))
			{
				DecimalRounding.RoudingType type;
				if (!Enum.TryParse<RoudingType>(matchType, true, out type))
				{
					throw new HttpLikeException(HttpStatusCode.BadRequest, string.Format("Unknown rounding strategy '{0}'", matchType));
				}

				rouding.Type = type;
			}

			if (!String.IsNullOrEmpty(matchDecimals))
			{
				int numberOfDecimals;
				if (!int.TryParse(matchDecimals, out numberOfDecimals))
				{
					throw new HttpLikeException(HttpStatusCode.BadRequest, string.Format("Bad number of decimals value '{0}'", matchDecimals));
				}

				rouding.NumberOfDecimals = numberOfDecimals;
			}

			return rouding;
		}
	}
}
