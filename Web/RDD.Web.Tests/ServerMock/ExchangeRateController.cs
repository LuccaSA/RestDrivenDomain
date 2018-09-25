using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RDD.Application;
using RDD.Domain.Helpers;
using RDD.Web.Controllers;
using RDD.Web.Helpers;
using RDD.Web.Querying;

namespace RDD.Web.Tests.ServerMock
{
    [Route("ExchangeRate")]
    public class ExchangeRateController : WebController<ExchangeRate, int>
    {
        public ExchangeRateController(IAppController<ExchangeRate, int> appController, ICandidateFactory<ExchangeRate, int> helper, IQueryFactory queryFactory)
            : base(appController, helper, queryFactory)
        {
        }

        protected override HttpVerbs AllowedHttpVerbs => HttpVerbs.All;


        [HttpGet]
        public override Task<ActionResult<IEnumerable<ExchangeRate>>> GetAsync() => base.GetAsync();

        [HttpGet("{id}")]
        public override Task<ActionResult<ExchangeRate>> GetByIdAsync(int id) => base.GetByIdAsync(id);

        [HttpPut("{id}")]
        public override Task<ActionResult<ExchangeRate>> PutByIdAsync(int id) => base.PutByIdAsync(id);

        [HttpPut]
        public override Task<ActionResult<IEnumerable<ExchangeRate>>> PutAsync() => base.PutAsync();

        [HttpPost]
        public override Task<ActionResult<ExchangeRate>> PostAsync() => base.PostAsync();

        [HttpDelete("{id}")]
        public override Task<ActionResult> DeleteByIdAsync(int id) => base.DeleteByIdAsync(id);
    }

    [Route("NonRdd")]
    public class NonRddController : ControllerBase
    {
        [HttpGet]
        public ActionResult<NonRddObject> GetData() => Ok(new NonRddObject { Data = "Hello World" });
    }
    
    public class NonRddObject
    {
        public string Data { get; set; }
    }
}