using Microsoft.AspNetCore.Mvc;
using Rdd.Application;
using Rdd.Infra.Web.Models;
using Rdd.Web.Controllers;
using Rdd.Web.Querying;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rdd.Web.Tests.Models
{
    [Route("Users")]
    public class UserWebController : ReadOnlyWebController<IUser, int>
    {
        public UserWebController(IReadOnlyAppController<IUser, int> appController, IQueryParser<IUser> queryParser)
            : base(appController, queryParser) { }

        //This method only intend is to check that IUser constraint on ReadOnlyWebController is sufficient and working
        public async Task<IEnumerable<IUser>> GetEnumerableAsync()
        {
            var query = new Query<IUser>();
            query.Options.CheckRights = false; //Don't care about rights check

            return (await AppController.GetAsync(query)).Items;
        }
    }
}
