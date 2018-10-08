using Rdd.Domain.Models.Querying;
using System;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class GuidHelperTests
    {
        private readonly StringConverter _helper;

        public GuidHelperTests()
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
    }
}