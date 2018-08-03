using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RDD.Application;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Web.Controllers;
using RDD.Web.Helpers;
using RDD.Web.Serialization;

namespace RDD.Web.Tests.ServerMock
{
    public class ExchangeRate : EntityBase<ExchangeRate, int>
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
    }

    [Route("ExchangeRate")]
    public class ExchangeRateController : WebController<ExchangeRate,int>
    {
        public ExchangeRateController(IAppController<ExchangeRate, int> appController, ApiHelper<ExchangeRate, int> helper, IRDDSerializer rddSerializer)
            : base(appController, helper, rddSerializer)
        {
        }

        protected override HttpVerbs AllowedHttpVerbs => HttpVerbs.All;

        [HttpGet]
        public override Task<IActionResult> GetAsync()
        {
            return base.GetAsync();
        }

    }
}