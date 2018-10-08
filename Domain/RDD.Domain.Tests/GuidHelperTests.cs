using Rdd.Domain.Helpers;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class GuidHelperTests
    {
        private readonly GuidHelper _helper;

        public GuidHelperTests()
        {
            _helper = new GuidHelper();
        }

        [Theory]
        [InlineData("aabbccdd-eeff", "Incomplete Guid with dashes")]
        [InlineData("aabbccdd-eeff-1111-2222-333333333333", "Full Guid")]
        [InlineData("aabbccddeeff", "Incomplete Guid without dash")]
        public void InterpreteStringAsGuid_WHEN_WellFormedStringGuid(string input, string label)
        {
            _helper.Complete(input);
        }
    }
}
