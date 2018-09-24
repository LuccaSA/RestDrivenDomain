using RDD.Application;
using RDD.Domain.Helpers;
using RDD.Web.Controllers;
using RDD.Web.Helpers;
using RDD.Web.Querying;

namespace RDD.Web.Tests.ServerMock
{
    public class ExchangeRateController : WebController<ExchangeRate, int>
    {
        public ExchangeRateController(IAppController<ExchangeRate, int> appController, ICandidateFactory<ExchangeRate, int> helper, IQueryFactory queryFactory)
            : base(appController, helper, queryFactory)
        {
        }

        protected override HttpVerbs AllowedHttpVerbs => HttpVerbs.All;
    }
}