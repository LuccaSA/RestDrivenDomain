using RDD.Application;
using RDD.Domain.Helpers;
using RDD.Web.Controllers;
using RDD.Web.Helpers;

namespace RDD.Web.Tests.Models
{
    public class IUserWebController : ReadOnlyWebController<IUser, int>
    {
        public IUserWebController(IReadOnlyAppController<IUser, int> appController, ApiHelper<IUser, int> apiHelper)
            : base(appController, apiHelper)
        {
        }

        protected override HttpVerbs AllowedHttpVerbs => HttpVerbs.Get;
    }
}