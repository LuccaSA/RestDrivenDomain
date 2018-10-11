using Rdd.Domain.Models;

namespace Rdd.Web.Tests.ServerMock
{
    public class ExchangeRate : EntityBase<int>
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
    }
}