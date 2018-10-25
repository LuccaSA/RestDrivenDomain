using Microsoft.AspNetCore.Mvc;
using Rdd.Domain;
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
        public UserWebController(IReadOnlyRepository<IUser, int> repository, IQueryParser<IUser, int> queryParser, HttpQuery<IUser, int> httpQuery)
            : base(repository, queryParser, httpQuery) { }

        //This method only intend is to check that IUser constraint on ReadOnlyWebController is sufficient and working
        public async Task<IEnumerable<IUser>> GetEnumerableAsync()
        {
            var query = new HttpQuery<IUser, int>();
            query.Options.CheckRights = false; //Don't care about rights check

            return (await Repository.GetAsync()).Items;
        }
    }
}
