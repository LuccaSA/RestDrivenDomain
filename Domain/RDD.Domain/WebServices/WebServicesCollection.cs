using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Rights;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Domain.WebServices
{
    public class WebServicesCollection : ReadOnlyRestCollection<WebService, int>, IWebServicesCollection
    {
        public WebServicesCollection(IRepository<WebService> repository, IRightsService rightsService)
            : base(repository, rightsService) { }

        public async Task<IEnumerable<WebService>> GetByTokenAsync(string token)
        {
            return (await GetAsync(new Query<WebService>(ws => ws.Token == token)))
                .Items;
        }
    }
}
