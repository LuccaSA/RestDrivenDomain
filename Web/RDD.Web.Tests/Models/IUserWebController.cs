using RDD.Application;
using RDD.Domain.Helpers;
using RDD.Web.Controllers;
using RDD.Web.Helpers;
using RDD.Web.Querying;

namespace RDD.Web.Tests.Models
{
    public class IUserWebController : ReadOnlyWebController<IUser, int>
    {
        public IUserWebController(IReadOnlyAppController<IUser, int> appController, ICandidateFactory<IUser, int> candidateFactory, IQueryFactory queryFactory)
            : base(appController, candidateFactory, queryFactory)
        {
        }

        protected override HttpVerbs AllowedHttpVerbs => HttpVerbs.Get;
    }
}