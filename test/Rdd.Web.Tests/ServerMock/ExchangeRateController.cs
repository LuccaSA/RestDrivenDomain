using Microsoft.AspNetCore.Mvc;
using Rdd.Application;
using Rdd.Domain.Helpers;
using Rdd.Domain.Models.Querying;
using Rdd.Web.Controllers;
using Rdd.Web.Querying;
using System.Threading.Tasks;

namespace Rdd.Web.Tests.ServerMock
{
    [ApiExplorerSettings(IgnoreApi = false)]
    public class ExchangeRate3Controller : ReadOnlyWebController<ExchangeRate, int>
    {
        public ExchangeRate3Controller(IAppController<ExchangeRate, int> appController, IQueryParser<ExchangeRate> queryParser)
            : base(appController, queryParser)
        {
        }
        protected override HttpVerbs AllowedHttpVerbs => HttpVerbs.All;
    }

    [Route(RouteName)]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class ExchangeRate2Controller : WebController<ExchangeRate2, int>
    {
        public const string RouteName = "OpenExchangeRates";

        public ExchangeRate2Controller(IAppController<ExchangeRate2, int> appController, ICandidateParser candidateParser, IQueryParser<ExchangeRate2> queryParser)
            : base(appController, candidateParser, queryParser)
        {
        }
        protected override HttpVerbs AllowedHttpVerbs => HttpVerbs.All;
    }

    [Route("ExchangeRates")]
    public class ExchangeRateController : WebController<ExchangeRate, int>
    {
        public ExchangeRateController(IAppController<ExchangeRate, int> appController, ICandidateParser candidateParser, IQueryParser<ExchangeRate> queryParser)
            : base(appController, candidateParser, queryParser)
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