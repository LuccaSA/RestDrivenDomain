using RDD.Domain.WebServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDD.Domain
{
    public interface IWebServicesCollection : IReadOnlyRestCollection<WebService, int>
    {
        Task<IEnumerable<WebService>> GetByTokenAsync(string token);
    }
}
