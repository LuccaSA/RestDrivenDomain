using RDD.Application;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Web.Controllers;
using RDD.Web.Helpers;
using RDD.Web.Querying;
using RDD.Web.Serialization;

namespace RDD.Web.Tests.ServerMock
{
    public class ExchangeRate : EntityBase<ExchangeRate, int>
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
    }

    public class ExchangeRateController : WebController<ExchangeRate,int>
    {
        public ExchangeRateController(IAppController<ExchangeRate, int> appController, ICandidateFactory<ExchangeRate, int> helper, IQueryFactory queryFactory)
            : base(appController, helper, queryFactory)
        {
        }

        protected override HttpVerbs AllowedHttpVerbs => HttpVerbs.All;
    }
}