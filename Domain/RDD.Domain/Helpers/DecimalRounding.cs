using RDD.Domain.Exceptions;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace RDD.Domain.Helpers
{
    public class DecimalRounding
    {
        public static readonly DecimalRounding Default = new DecimalRounding(RoudingType.Floor);

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

        public DecimalRounding(RoudingType type, int numberOfDecimals = 0)
        {
            if ((type == RoudingType.Floor || type == RoudingType.Ceiling) && numberOfDecimals != 0)
            {
                throw new Exception(String.Format("Does not support decimals with {0} rounding strategy", type));
            }

            Type = type;
            NumberOfDecimals = numberOfDecimals;
        }

        public Func<double, double> GetDoubleRoundingFunction()
        {
            switch (Type)
            {
                case RoudingType.Round:
                    return (d) => Math.Round(d, NumberOfDecimals, MidpointRounding.AwayFromZero);

                case RoudingType.RoundEven:
                    return (d) => Math.Round(d, NumberOfDecimals, MidpointRounding.ToEven);

                case RoudingType.Ceiling:
                    return Math.Ceiling;

                case RoudingType.Floor:
                    return Math.Floor;

                default:
                    throw new HttpLikeException(HttpStatusCode.BadRequest, string.Format("Unknown rounding strategy '{0}'", Type.ToString()));
            }
        }

        public Func<decimal, decimal> GetDecimalRoundingFunction()
        {
            switch (Type)
            {
                case RoudingType.Round:
                    return (d) => Math.Round(d, NumberOfDecimals, MidpointRounding.AwayFromZero);

                case RoudingType.RoundEven:
                    return (d) => Math.Round(d, NumberOfDecimals, MidpointRounding.ToEven);

                case RoudingType.Ceiling:
                    return Math.Ceiling;

                case RoudingType.Floor:
                    return Math.Floor;

                default:
                    throw new HttpLikeException(HttpStatusCode.BadRequest, string.Format("Unknown rounding strategy '{0}'", Type.ToString()));
            }
        }

        public static DecimalRounding Parse(string pattern)
        {
            var matches = Regex.Match(pattern, "sum\\(([a-zA-Z0-9_]*),?([a-zA-Z0-9_]*),?([a-zA-Z0-9_]*)\\)").Groups;
            var matchType = matches[2].Value;
            var matchDecimals = matches[3].Value;

            var rouding = Default;

            if (!String.IsNullOrEmpty(matchType))
            {
                RoudingType type;
                if (!Enum.TryParse(matchType, true, out type))
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
