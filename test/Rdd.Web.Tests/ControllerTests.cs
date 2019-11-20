using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Rdd.Application;
using Rdd.Domain;
using Rdd.Domain.Exceptions;
using Rdd.Domain.Models.Querying;
using Rdd.Web.Querying;
using Rdd.Web.Tests.ServerMock;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Web.Tests
{
    public class ControllerTests
    {
        [Fact]
        public async Task ForbiddenTests()
        {
            var appController = new Mock<IAppController<ExchangeRate2, int>>();
            appController.Setup(a => a.CreateAsync(It.IsAny<ICandidate<ExchangeRate2, int>>(), It.IsAny<Query<ExchangeRate2>>())).ThrowsAsync(new ForbiddenException(""));
            var controller = new ExchangeRate2Controller(appController.Object, new Mock<ICandidateParser>().Object, new Mock<IQueryParser<ExchangeRate2>>().Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var result = await controller.Post();
            Assert.IsType<ForbidResult>(result);
        }
    }
}
