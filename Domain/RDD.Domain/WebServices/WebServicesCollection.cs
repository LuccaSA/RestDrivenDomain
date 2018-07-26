using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Domain.WebServices
{
    public class WebServicesCollection : ReadOnlyRestCollection<WebService, int>, IWebServicesCollection
    {
        public WebServicesCollection(IRepository<WebService> repository, QueryContext queryContext)
            : base(repository, queryContext) { }

        public async Task<IEnumerable<WebService>> GetByTokenAsync(string token)
        {
            return await GetAsync(new Query<WebService>(ws => ws.Token == token));
        }
    }
}
