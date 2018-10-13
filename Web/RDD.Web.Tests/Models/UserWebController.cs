using Microsoft.AspNetCore.Mvc;
using Rdd.Application;
using Rdd.Web.Controllers;
using Rdd.Web.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rdd.Domain.Models.Querying;

namespace Rdd.Web.Tests.Models
{
    [Route("Users")]
    public class UserWebController : ReadOnlyWebController<IUser, int>
    {
        public UserWebController(IReadOnlyAppController<IUser, int> appController, ApiHelper<IUser, int> apiHelper)
            : base(appController, apiHelper) { }

        //This method only intend is to check that IUser constraint on ReadOnlyWebController is sufficient and working
        public async Task<IEnumerable<IUser>> GetEnumerableAsync()
        {
            var query = new Query<IUser>();
            query.Options.CheckRights = false; //Don't care about rights check

            return (await AppController.GetAsync(query)).Items;
        }
    }
}
