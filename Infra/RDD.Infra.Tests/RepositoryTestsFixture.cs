using Moq;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Infra.Contexts;
using RDD.Infra.Storage;
using RDD.Infra.Tests.Models;
using RDD.Infra.Tests.Repositories;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Infra.Tests
{
    public class RepositoryTestsFixture
    {
        protected Container Container;

        public RepositoryTestsFixture()
        {
            Container = new Container();
            Container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            Container.Options.AllowOverridingRegistrations = true;

            Container.Register<IStorageService, InMemoryStorageService>(Lifestyle.Scoped);

            Container.Register<UsersRepository>(Lifestyle.Scoped);

            Container.Register<IExecutionContext>(() =>
            {
                var executionContext = new HttpExecutionContext();
                var principal = new Mock<IPrincipal>();
                principal.Setup(p => p.HasOperation(It.IsAny<int>()))
                    .Returns(true);
                principal.Setup(p => p.HasAnyOperations(It.IsAny<HashSet<int>>()))
                    .Returns(true);
                principal.Setup(p => p.ApplyRights<User>(It.IsAny<IQueryable<User>>(), It.IsAny<HashSet<int>>()))
                    .Returns<IQueryable<User>, HashSet<int>>((entities, operations) => entities);

                executionContext.curPrincipal = principal.Object;

                return executionContext;
            }, Lifestyle.Scoped);
            Container.Register<ICombinationsHolder>(() =>
            {
                var combinationHolder = new Mock<ICombinationsHolder>();
                combinationHolder.Setup(c => c.Combinations)
                    .Returns(new List<Combination>
                    {
                        new Combination { Operation = new Operation { Id = 1 }, Verb = HttpVerbs.Get, Subject = typeof(User) }
                    });

                return combinationHolder.Object;
            }, Lifestyle.Scoped);
        }

        public Scope GetScope()
        {
            return AsyncScopedLifestyle.BeginScope(Container);
        }
    }
}
