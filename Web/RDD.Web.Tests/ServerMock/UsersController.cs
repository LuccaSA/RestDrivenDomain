using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RDD.Application;
using RDD.Domain.Helpers;
using RDD.Web.Controllers;
using RDD.Web.Helpers;
using RDD.Web.Querying;
using RDD.Web.Tests.Models;

namespace RDD.Web.Tests.ServerMock
{
    [Route("Users")]
    public class UsersController : WebController<User, int>
    {
        public UsersController(IAppController<User, int> appController, ICandidateFactory<User, int> helper, IQueryFactory queryFactory)
            : base(appController, helper, queryFactory) { }

        protected override HttpVerbs AllowedHttpVerbs => HttpVerbs.All;

        [HttpGet]
        public override Task<ActionResult<IEnumerable<User>>> GetAsync() => base.GetAsync();

        [HttpGet("{id}")]
        public override Task<ActionResult<User>> GetByIdAsync(int id) => base.GetByIdAsync(id);
    }
}