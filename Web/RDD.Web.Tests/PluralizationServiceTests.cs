using RDD.Web.Serialization;
using Xunit;

namespace RDD.Web.Tests
{
    public class PluralizationServiceTests
    {
        private readonly PluralizationService _service;

        public PluralizationServiceTests()
        {
            _service = new PluralizationService();
        }

        [Theory]
        [InlineData("legalEntity", "legalEntities")]
        [InlineData("user", "users")]
        [InlineData("employee", "employees")]
        public void Plurals_should_work(string singular, string plural)
        {
            var result = _service.GetPlural(singular);

            Assert.Equal(plural, result);
        }
    }
}
