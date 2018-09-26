using RDD.Domain.Models;

namespace RDD.Web.Tests.ServerMock
{
    public class ExchangeRate : EntityBase<ExchangeRate, int>
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
    }
}