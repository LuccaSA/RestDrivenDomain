using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RDD.Domain.Tests
{
    public class DecimalRoundingTests
    {
        [Fact]
        public void SHOULD_round_to_defaut_WHEN_empy_rounding_pattern()
        {
            var pattern = "sum(id)";

            var rounding = DecimalRounding.Parse(pattern);

            Assert.Equal(DecimalRounding.Default, rounding);
        }

        [Theory]
        [InlineData(DecimalRounding.RoudingType.Round)]
        [InlineData(DecimalRounding.RoudingType.RoundEven)]
        [InlineData(DecimalRounding.RoudingType.Floor)]
        [InlineData(DecimalRounding.RoudingType.Ceiling)]
        public void SHOULD_round_to_value_WHEN_present_in_rounding_pattern(DecimalRounding.RoudingType strategy)
        {
            var pattern = String.Format("sum(id,{0})", strategy.ToString().ToLower());

            var rounding = DecimalRounding.Parse(pattern);

            Assert.Equal(strategy, rounding.Type);
        }

        [Theory]
        [InlineData(DecimalRounding.RoudingType.Round, 0)]
        [InlineData(DecimalRounding.RoudingType.Round, 1)]
        [InlineData(DecimalRounding.RoudingType.Round, 2)]
        [InlineData(DecimalRounding.RoudingType.Round, 3)]
        [InlineData(DecimalRounding.RoudingType.Ceiling, 0)]
        [InlineData(DecimalRounding.RoudingType.Ceiling, 1)]
        [InlineData(DecimalRounding.RoudingType.Ceiling, 2)]
        [InlineData(DecimalRounding.RoudingType.Ceiling, 3)]
        public void SHOULD_round_to_value_WHEN_present_in_rounding_pattern_with_decimals(DecimalRounding.RoudingType strategy, int numberOfDecimals)
        {
            var pattern = String.Format("sum(id,{0},{1})", strategy.ToString().ToLower(), numberOfDecimals);

            var rounding = DecimalRounding.Parse(pattern);

            Assert.Equal(strategy, rounding.Type);
            Assert.Equal(numberOfDecimals, rounding.NumberOfDecimals);
        }

        [Fact]
        public void SHOULD_round_to_two_decimal_WHEN_asked()
        {
            var items = new HashSet<User>() { new User { Salary = 12.34M }, new User { Salary = 45.67M } };
            var selection = new Selection<User>(items, 2);

            var result = selection.Sum(typeof(User).GetProperty("Salary"), new DecimalRounding(DecimalRounding.RoudingType.Round, 2));

            Assert.Equal(58.01M, result);

            result = selection.Sum(typeof(User).GetProperty("Salary"), new DecimalRounding(DecimalRounding.RoudingType.Round, 1));

            Assert.Equal(58.0M, result);

            result = selection.Sum(typeof(User).GetProperty("Salary"), new DecimalRounding(DecimalRounding.RoudingType.Round));

            Assert.Equal(58M, result);

            result = selection.Sum(typeof(User).GetProperty("Salary"), new DecimalRounding(DecimalRounding.RoudingType.Floor));

            Assert.Equal(58M, result);

            result = selection.Sum(typeof(User).GetProperty("Salary"), new DecimalRounding(DecimalRounding.RoudingType.Ceiling));

            Assert.Equal(59M, result);
        }
    }
}
