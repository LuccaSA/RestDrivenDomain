using Rdd.Domain.Models.Querying;
using Rdd.Domain.Tests.Models;
using System;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class StringConverterTests
    {
        private readonly StringConverter _helper;

        public StringConverterTests()
        {
            _helper = new StringConverter();
        }

        [Theory]
        [InlineData("aabbccdd-eeff")]
        [InlineData("aabbccdd-eeff-1111-2222-333333333333")]
        [InlineData("aabbccddeeff")]
        public void InterpreteStringAsGuid_WHEN_WellFormedStringGuid(string input)
        {
            _helper.ConvertTo<Guid>(input);
        }

        [Theory]
        [InlineData(typeof(string), "null")]
        [InlineData(typeof(string), null)]
        [InlineData(typeof(double?), "")]
        [InlineData(typeof(double?), "null")]
        [InlineData(typeof(double?), null)]
        [InlineData(typeof(DateTime?), "")]
        [InlineData(typeof(DateTime?), "null")]
        [InlineData(typeof(DateTime?), null)]
        [InlineData(typeof(User), "")]
        [InlineData(typeof(User), "null")]
        [InlineData(typeof(User), null)]
        public void InterpretNull(Type type, string input)
        {
            Assert.Null(_helper.ConvertTo(type, input));
        }

        [Theory]
        [InlineData("1200.23")]
        [InlineData("1200,23")]
        [InlineData("1200.23e-0")]
        [InlineData("1200,23e-0")]
        public void Double(string input)
        {
            decimal resultM = 1200.23M;
            double resultD = 1200.23D;

            Assert.Equal(resultD, _helper.ConvertTo(typeof(double), input));
            Assert.Equal(resultM, _helper.ConvertTo(typeof(decimal), input));
        }

        [Fact]
        public void Dates()
        {
            Assert.Equal(DateTime.Today, _helper.ConvertTo(typeof(DateTime), "today"));
            Assert.Equal(DateTime.Today, _helper.ConvertTo(typeof(DateTime?), "today"));

            Assert.InRange((DateTime)_helper.ConvertTo(typeof(DateTime), "now"), DateTime.Now.AddMilliseconds(-1), DateTime.Now);
            Assert.InRange((DateTime)_helper.ConvertTo(typeof(DateTime?), "now"), DateTime.Now.AddMilliseconds(-1), DateTime.Now);

            Assert.Equal(DateTime.Today.AddDays(1), _helper.ConvertTo(typeof(DateTime), "tomorrow"));
            Assert.Equal(DateTime.Today.AddDays(1), _helper.ConvertTo(typeof(DateTime?), "tomorrow"));

            Assert.Equal(DateTime.Today.AddDays(-1), _helper.ConvertTo(typeof(DateTime), "yesterday"));
            Assert.Equal(DateTime.Today.AddDays(-1), _helper.ConvertTo(typeof(DateTime?), "yesterday"));
        }

        enum MyEnum
        {
            aa = 0,
            bb = 1
        }

        [Fact]
        public void SpecialCases()
        {
            Assert.Equal(MyEnum.bb, _helper.ConvertTo(typeof(MyEnum), "bb"));
            Assert.Equal(MyEnum.bb, _helper.ConvertTo(typeof(MyEnum), "1"));
            Assert.Equal(MyEnum.bb, _helper.ConvertTo(typeof(MyEnum?), "bb"));
            Assert.Equal(MyEnum.bb, _helper.ConvertTo(typeof(MyEnum?), "1"));
            Assert.Null(_helper.ConvertTo(typeof(MyEnum?), ""));
            Assert.Null(_helper.ConvertTo(typeof(MyEnum?), "null"));
            Assert.Null(_helper.ConvertTo(typeof(MyEnum?), null));

            Assert.Equal(new TimeSpan(1, 0, 0), _helper.ConvertTo(typeof(TimeSpan), "01:00:00"));
            Assert.Equal(new TimeSpan(1, 0, 0), _helper.ConvertTo(typeof(TimeSpan?), "01:00:00"));

            Assert.Equal(new Uri("http://www.example.com"), _helper.ConvertTo(typeof(Uri), "http://www.example.com"));
            Assert.Equal(string.Empty, _helper.ConvertTo(typeof(string), ""));
        }
    }
}