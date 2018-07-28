using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Domain.WebServices
{
    public class WebServicesCollection : ReadOnlyRestCollection<WebService, int>, IWebServicesCollection
    {
        public WebServicesCollection(IReadOnlyRepository<WebService> repository)
            : base(repository)
        {
        }

        public Task<IEnumerable<WebService>> GetByTokenAsync(string token) 
            => GetAsync(new Query<WebService>(ws => ws.Token == token));
    }
}
