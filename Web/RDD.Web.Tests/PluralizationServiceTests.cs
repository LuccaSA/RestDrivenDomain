using RDD.Web.Serialization.UrlProviders;
using Xunit;

namespace RDD.Web.Tests
{
    public class PluralizationServiceTests
    {
        private readonly PluralizationService _service;

        public PluralizationServiceTests()
        {
            _service = new PluralizationService(new Inflector.Inflector(new System.Globalization.CultureInfo("en-US")));
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
