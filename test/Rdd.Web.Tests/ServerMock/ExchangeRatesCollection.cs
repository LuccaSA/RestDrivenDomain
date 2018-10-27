using Rdd.Domain;
using Rdd.Domain.Models;
using Rdd.Domain.Patchers;
using System.Threading.Tasks;

namespace Rdd.Web.Tests.ServerMock
{
    public class ExchangeRatesCollection : RestCollection<ExchangeRate, int>
    {
        public ExchangeRatesCollection(IRepository<ExchangeRate, int> repository, IPatcher<ExchangeRate> patcher)
            : base(repository, patcher) { }

        public override Task<ExchangeRate> InstantiateEntityAsync(ICandidate<ExchangeRate, int> candidate)
        {
            return Task.FromResult(new ExchangeRate());
        }
    }
}
