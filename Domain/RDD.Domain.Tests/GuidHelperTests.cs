using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Tests.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace RDD.Domain.Tests
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
