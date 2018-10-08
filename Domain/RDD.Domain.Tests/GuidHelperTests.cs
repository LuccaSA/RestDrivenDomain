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
        [InlineData("aabbccdd-eeff")]
        [InlineData("aabbccdd-eeff-1111-2222-333333333333")]
        [InlineData("aabbccddeeff")]
        public void InterpreteStringAsGuid_WHEN_WellFormedStringGuid(string input)
        {
            _helper.Complete(input);
        }
    }
}
