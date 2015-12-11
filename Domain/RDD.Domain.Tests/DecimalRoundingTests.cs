using NUnit.Framework;
using RDD.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Tests
{
	public class DecimalRoundingTests
	{
		[Test]
		public void Should_round_to_defaut_when_empy_rounding_pattern()
		{
			var pattern = "sum(id)";

			var rounding = DecimalRounding.Parse(pattern);

			Assert.AreEqual(DecimalRounding.Default, rounding);
		}

		[TestCase(DecimalRounding.RoudingType.Round)]
		[TestCase(DecimalRounding.RoudingType.RoundEven)]
		[TestCase(DecimalRounding.RoudingType.Floor)]
		[TestCase(DecimalRounding.RoudingType.Ceiling)]
		public void Should_round_to_value_when_present_in_rounding_pattern(DecimalRounding.RoudingType strategy)
		{
			var pattern = String.Format("sum(id,{0})", strategy.ToString().ToLower());

			var rounding = DecimalRounding.Parse(pattern);

			Assert.AreEqual(strategy, rounding.Type);
		}

		[TestCase(DecimalRounding.RoudingType.Round, 0)]
		[TestCase(DecimalRounding.RoudingType.Round, 1)]
		[TestCase(DecimalRounding.RoudingType.Round, 2)]
		[TestCase(DecimalRounding.RoudingType.Round, 3)]
		[TestCase(DecimalRounding.RoudingType.Ceiling, 0)]
		[TestCase(DecimalRounding.RoudingType.Ceiling, 1)]
		[TestCase(DecimalRounding.RoudingType.Ceiling, 2)]
		[TestCase(DecimalRounding.RoudingType.Ceiling, 3)]
		public void Should_round_to_value_when_present_in_rounding_pattern(DecimalRounding.RoudingType strategy, int numberOfDecimals)
		{
			var pattern = String.Format("sum(id,{0},{1})", strategy.ToString().ToLower(), numberOfDecimals);

			var rounding = DecimalRounding.Parse(pattern);

			Assert.AreEqual(strategy, rounding.Type);
			Assert.AreEqual(numberOfDecimals, rounding.NumberOfDecimals);
		}
	}
}
