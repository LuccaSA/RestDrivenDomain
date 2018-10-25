using Microsoft.AspNetCore.Mvc;
using Rdd.Application;
using Rdd.Domain;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using Rdd.Infra.Web.Models;
using Rdd.Web.Controllers;
using Rdd.Web.Querying;
using System.Threading.Tasks;

namespace Rdd.Web.Tests.ServerMock
{
    [ApiExplorerSettings(IgnoreApi = false)]
    public class ExchangeRate3Controller : ReadOnlyWebController<ExchangeRate, int>
    {
        public ExchangeRate3Controller(IReadOnlyRepository<ExchangeRate, int> repository, IQueryParser<ExchangeRate, int> queryParser, HttpQuery<ExchangeRate, int> httpQuery)
            : base(repository, queryParser, httpQuery)
        {
        }
        protected override HttpVerbs AllowedHttpVerbs => HttpVerbs.All;
    }

    [Route(RouteName)]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class ExchangeRate2Controller : WebController<ExchangeRate2, int>
    {
        public const string RouteName = "OpenExchangeRates";

        public ExchangeRate2Controller(IAppController<ExchangeRate2, int> appController, IRepository<ExchangeRate2, int> repository, ICandidateParser candidateParser, IQueryParser<ExchangeRate2, int> queryParser, HttpQuery<ExchangeRate2, int> query)
            : base(appController, repository, candidateParser, queryParser, query)
        {
        }
        protected override HttpVerbs AllowedHttpVerbs => HttpVerbs.All;
    }

    [Route("ExchangeRates")]
    public class ExchangeRateController : WebController<ExchangeRate, int>
    {
        public ExchangeRateController(IAppController<ExchangeRate, int> appController, IRepository<ExchangeRate, int> repository, ICandidateParser candidateParser, IQueryParser<ExchangeRate, int> queryParser, HttpQuery<ExchangeRate, int> query)
            : base(appController, repository, candidateParser, queryParser, query)
        {
        }

        //for route testing
        public static HttpVerbs ConfigurableAllowedHttpVerbs = HttpVerbs.Get | HttpVerbs.Post | HttpVerbs.Put;
        protected override HttpVerbs AllowedHttpVerbs => ConfigurableAllowedHttpVerbs;

        [HttpPost("creation")]//testing route override
        public override Task<IActionResult> PostAsync()
            => base.PostAsync();

        [HttpGet("customRoute")]
        public void GetWithParams([FromQuery]int pipo, int pipo2)
        {
            QueryParser.Parse(HttpContext.Request, ControllerContext.ActionDescriptor, true);
        }
    }
}