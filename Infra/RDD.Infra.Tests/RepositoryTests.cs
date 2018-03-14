using RDD.Domain.Models.Querying;
using RDD.Infra.Tests.Models;
using RDD.Infra.Tests.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RDD.Infra.Tests
{
    public class RepositoryTests : IClassFixture<RepositoryTestsFixture>
    {
        private readonly RepositoryTestsFixture _fixture;

        public RepositoryTests(RepositoryTestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AddingToRepositoryShoulIncrementCount()
        {
            using (var scope = _fixture.GetScope())
            {
                var repo = scope.Container.GetInstance<UsersRepository>();

                repo.Add(new User());
                repo.Add(new User());

                Assert.Equal(2, await repo.CountAsync(new Query<User>()));
            }
        }
    }
}
