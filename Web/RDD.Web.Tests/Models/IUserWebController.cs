using RDD.Application;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Web.Controllers;
using RDD.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Web.Tests.Models
{
    public class IUserWebController : ReadOnlyWebController<IUser, int>
    {
        public IUserWebController(IReadOnlyAppController<IUser, int> appController, ApiHelper<IUser, int> apiHelper)
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
