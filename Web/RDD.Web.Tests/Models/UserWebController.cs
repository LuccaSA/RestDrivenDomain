using Microsoft.AspNetCore.Mvc;
using RDD.Application;
using RDD.Web.Controllers;
using RDD.Web.Helpers;
using RDD.Web.Querying;
using RDD.Web.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Web.Tests.Models
{
    [Route("Users")]
    public class UserWebController : ReadOnlyWebController<IUser, int>
    {
        public UserWebController(IReadOnlyAppController<IUser, int> appController, ApiHelper<IUser, int> apiHelper, IRDDSerializer rddSerializer)
            : base(appController, apiHelper, rddSerializer) { }

        //This method only intend is to check that IUser constraint on ReadOnlyWebController is sufficient and working
        public async Task<IEnumerable<IUser>> GetEnumerableAsync()
        {
            var query = new WebQuery<IUser>();
            query.Options.CheckRights = false; //Don't care about rights check

            return (await AppController.GetAsync(query)).Items;
        }
    }
}
