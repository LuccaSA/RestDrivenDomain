﻿using Rdd.Domain.Tests.Models;
using Rdd.Infra.Rights;
using Rdd.Infra.Web.Models;
using System;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class RightExpressionsHelperTests
    {
        [Fact]
        public void RightExpressionsHelper()
        {
            var filter = new OpenRightExpressionsHelper<User>().GetFilter(new HttpQuery<User, Guid> { Verb = Helpers.HttpVerbs.Get });
            Assert.True(filter.Compile()(null));

            filter = new ClosedRightExpressionsHelper<User>().GetFilter(new HttpQuery<User, Guid> { Verb = Helpers.HttpVerbs.Get });
            Assert.False(filter.Compile()(null));
        }
    }
}