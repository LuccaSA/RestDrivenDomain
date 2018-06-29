using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Domain.WebServices
{
    public class WebServicesCollection : RestCollection<WebService, int>, IWebServicesCollection
    {
        public WebServicesCollection(IRepository<WebService> repository, IExecutionContext execution, ICombinationsHolder combinationsHolder)
            : base(repository, execution, combinationsHolder) { }

        public async Task<IEnumerable<WebService>> GetByTokenAsync(string token)
        {
            return await GetAsync(new ExpressionQuery<WebService>(ws => ws.Token == token));
        }
    }
}
