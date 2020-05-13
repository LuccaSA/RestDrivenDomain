using Rdd.Domain.Models.Querying;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rdd.Domain.Tests
{
    public class TypeFilterTests
    {
        class BaseClass
        {
        }
        class SuperClass : BaseClass
        {
        }

        [Fact]
        public void TypeFilterWorks()
        {
            var filter = TypeFilter<BaseClass>.FromType(typeof(SuperClass));

            Assert.Equal(typeof(TypeFilter<BaseClass, SuperClass>), filter.GetType());

            var entities = new List<BaseClass>
            {
                new BaseClass(),
                new SuperClass()
            }.AsQueryable();

            var filtered = filter.Apply(entities).ToList();

            Assert.Single(filtered);
            Assert.IsType<SuperClass>(filtered[0]);
        }
    }
}