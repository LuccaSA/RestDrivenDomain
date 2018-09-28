﻿using Microsoft.AspNetCore.Mvc;
using RDD.Application;
using RDD.Domain.Helpers;
using RDD.Web.Controllers;
using RDD.Web.Helpers;
using RDD.Web.Serialization;
using System.Threading.Tasks;

namespace RDD.Web.Tests.ServerMock
{
    [Route(RouteName)]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class ExchangeRate2Controller : WebController<ExchangeRate, int>
    {
        public const string RouteName = "OpenExchangeRate";

        public ExchangeRate2Controller(IAppController<ExchangeRate, int> appController, ApiHelper<ExchangeRate, int> helper, IRDDSerializer rddSerializer)
            : base(appController, helper, rddSerializer)
        {
        }
        protected override HttpVerb AllowedHttpVerbs => HttpVerb.All;
    }

    [Route("ExchangeRate")]
    public class ExchangeRateController : WebController<ExchangeRate, int>
    {
        public ExchangeRateController(IAppController<ExchangeRate, int> appController, ApiHelper<ExchangeRate, int> helper, IRDDSerializer rddSerializer)
            : base(appController, helper, rddSerializer)
        {
        }

        //for route testing
        public static HttpVerb ConfigurableAllowedHttpVerbs = HttpVerb.Get | HttpVerb.Post | HttpVerb.Put;
        protected override HttpVerb AllowedHttpVerbs => ConfigurableAllowedHttpVerbs;

        [HttpPost("creation")]//testing route override
        public override Task<IActionResult> PostAsync()
            => base.PostAsync();
    }
}