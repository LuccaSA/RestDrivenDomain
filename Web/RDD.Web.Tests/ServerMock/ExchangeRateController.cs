using Microsoft.AspNetCore.Mvc;
using RDD.Application;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Web.Controllers;
using RDD.Web.Querying;
using System.Threading.Tasks;

namespace RDD.Web.Tests.ServerMock
{
    [Route(RouteName)]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class ExchangeRate2Controller : WebController<ExchangeRate, int>
    {
        public const string RouteName = "OpenExchangeRate";

        public ExchangeRate2Controller(IAppController<ExchangeRate, int> appController, ICandidateParser candidateParser, IQueryParser<ExchangeRate> queryParser)
            : base(appController, candidateParser, queryParser)
        {
        }
        protected override HttpVerbs AllowedHttpVerbs => HttpVerbs.All;
    }

    [Route("ExchangeRate")]
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
    }
}