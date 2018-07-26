using Moq;
using RDD.Application.Controllers;
using RDD.Domain.Models;
using RDD.Infra.Storage;
using RDD.Web.Helpers;
using RDD.Web.Serialization;
using RDD.Web.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RDD.Domain.Models.Querying;
using Xunit;

namespace RDD.Web.Tests
{
    public class WebControllerTests
    {
        private readonly QueryContext _queryContex = new QueryContext(new QueryRequest(), new QueryResponse());
        [Fact]
        public async Task WebControllerShouldWorkOnInterfaces()
        {
            using (var storage = new InMemoryStorageService())
            {
                _queryContex.Request.CheckRights = false;
                var repository = new Repository<IUser>(storage, null, _queryContex.Request);
                var collection = new ReadOnlyRestCollection<IUser, int>(repository, _queryContex);
                var appController = new ReadOnlyAppController<IUser, int>(collection);

                repository.Add(new User { Id = 1 });
                repository.Add(new AnotherUser { Id = 2 });
                var ctxHelper = new HttpContextHelper(new HttpContextAccessor()
                {
                    HttpContext = new DefaultHttpContext()
                });
                var controller = new IUserWebController(appController, new ApiHelper<IUser, int>(ctxHelper));

                var results = await controller.GetAsync();

                OkObjectResult ok = (OkObjectResult)results.Result;
                IEnumerable<IUser> found = (IEnumerable<IUser>)ok.Value;

                Assert.Equal(2, found.Count());
            }
        }
    }
}
