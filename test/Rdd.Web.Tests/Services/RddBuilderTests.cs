using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rdd.Application;
using Rdd.Application.Controllers;
using Rdd.Domain;
using Rdd.Domain.Helpers.Expressions;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Json;
using Rdd.Domain.Models;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Patchers;
using Rdd.Domain.Rights;
using Rdd.Domain.Tests.Models;
using Rdd.Infra.Storage;
using Rdd.Web.Helpers;
using Rdd.Web.Querying;
using Rdd.Web.Serialization.Providers;
using Rdd.Web.Tests.ServerMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Rdd.Web.Tests.Services
{
    public class RddBuilderTests
    {
        public abstract class Hierarchy2 : IEntityBase<int>
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string Type { get; set; }
            public int Id { get; set; }

            public Hierarchy2 Clone() => this;
            public object GetId() => Id;
            public void SetId(object id) => Id = (int)id;
        }

        public class Super : Hierarchy2
        {
        }

        public class InheritanceConfiguration2 : IInheritanceConfiguration<Hierarchy2>
        {
            public Type BaseType => typeof(Hierarchy2);

            public string Discriminator => "type";

            public IReadOnlyDictionary<string, Type> Mappings => new Dictionary<string, Type>
            {
                { "super", typeof(Super) }
            };
        }

        [Fact]
        public void TestInheritanceRegister()
        {
            var services = new ServiceCollection();
            services.TryAddSingleton<IPatcherProvider, PatcherProvider>();
            services.TryAddSingleton<IReflectionHelper, ReflectionHelper>();

            new RddBuilder(services)
                .AddInheritanceConfiguration<InheritanceConfiguration, Hierarchy, int>(new InheritanceConfiguration())
                .AddInheritanceConfiguration<InheritanceConfiguration2, Hierarchy2, int>(new InheritanceConfiguration2());

            var provider = services.BuildServiceProvider();

            var configs = provider.GetRequiredService<IEnumerable<IInheritanceConfiguration>>();

            Assert.Equal(2, configs.ToList().Count);

            provider.GetRequiredService<IInheritanceConfiguration<Hierarchy>>();
            provider.GetRequiredService<IInheritanceConfiguration<Hierarchy2>>();

            Assert.IsType<BaseClassPatcher<Hierarchy>>(provider.GetRequiredService<IPatcher<Hierarchy>>());
            Assert.IsType<BaseClassInstanciator<Hierarchy>>(provider.GetRequiredService<IInstanciator<Hierarchy>>());
        }

        [Fact]
        public void TestEmptyInheritanceRegister()
        {
            var configs = new ServiceCollection().BuildServiceProvider().GetRequiredService<IEnumerable<IInheritanceConfiguration>>();

            Assert.Empty(configs);
        }

        class RepoPipo : IRepository<Hierarchy>
        {
            public void Add(Hierarchy entity) => throw new NotImplementedException();
            public void AddRange(IEnumerable<Hierarchy> entities) => throw new NotImplementedException();
            public Task<int> CountAsync(Query<Hierarchy> query) => throw new NotImplementedException();
            public void DiscardChanges(Hierarchy entity) => throw new NotImplementedException();
            public Task<IEnumerable<Hierarchy>> GetAsync(Query<Hierarchy> query) => throw new NotImplementedException();
            public Task<IEnumerable<Hierarchy>> PrepareAsync(IEnumerable<Hierarchy> entities, Query<Hierarchy> query) => throw new NotImplementedException();
            public void Remove(Hierarchy entity) => throw new NotImplementedException();
        }

        [Fact]
        public void TestReadOnlyRepoRegister()
        {
            var services = new ServiceCollection();
            new RddBuilder(services).AddReadOnlyRepository<RepoPipo, Hierarchy>();
            var provider = services.BuildServiceProvider();

            Assert.Null(provider.GetService<IRepository<Hierarchy>>());

            var repo2 = provider.GetRequiredService<IReadOnlyRepository<Hierarchy>>();
            var repo3 = provider.GetRequiredService<RepoPipo>();

            Assert.Equal(repo2, repo3);
        }

        [Fact]
        public void TestRepoRegister()
        {
            var services = new ServiceCollection();
            new RddBuilder(services).AddRepository<RepoPipo, Hierarchy>();
            var provider = services.BuildServiceProvider();

            var repo = provider.GetRequiredService<IRepository<Hierarchy>>();
            var repo2 = provider.GetRequiredService<IReadOnlyRepository<Hierarchy>>();
            var repo3 = provider.GetRequiredService<RepoPipo>();

            Assert.Equal(repo, repo2);
            Assert.Equal(repo, repo3);
            Assert.Equal(repo2, repo3);
        }

        class CollectionPipo : IRestCollection<Hierarchy, int>
        {
            public Task<bool> AnyAsync(Query<Hierarchy> query) => throw new NotImplementedException();

            public Task<Hierarchy> CreateAsync(ICandidate<Hierarchy, int> candidate, Query<Hierarchy> query = null) => throw new NotImplementedException();
            public Task<IEnumerable<Hierarchy>> CreateAsync(IEnumerable<ICandidate<Hierarchy, int>> candidates, Query<Hierarchy> query = null) => throw new NotImplementedException();
            public Task<IEnumerable<Hierarchy>> CreateAsync(IEnumerable<Hierarchy> created) => throw new NotImplementedException();

            public Task DeleteByIdAsync(int id) => throw new NotImplementedException();
            public Task DeleteByIdsAsync(IEnumerable<int> ids) => throw new NotImplementedException();

            public Task<ISelection<Hierarchy>> GetAsync(Query<Hierarchy> query) => throw new NotImplementedException();
            public Task<Hierarchy> GetByIdAsync(int id, Query<Hierarchy> query) => throw new NotImplementedException();

            public Task<Hierarchy> UpdateByIdAsync(int id, ICandidate<Hierarchy, int> candidate, Query<Hierarchy> query = null) => throw new NotImplementedException();
            public Task<IEnumerable<Hierarchy>> UpdateByIdsAsync(IDictionary<int, ICandidate<Hierarchy, int>> candidatesByIds, Query<Hierarchy> query = null) => throw new NotImplementedException();
        }

        [Fact]
        public void TestReadOnlyCollectionRegister()
        {
            var services = new ServiceCollection();
            new RddBuilder(services).AddReadOnlyRestCollection<CollectionPipo, Hierarchy, int>();
            var provider = services.BuildServiceProvider();

            Assert.Null(provider.GetService<IRestCollection<Hierarchy, int>>());

            var collection2 = provider.GetRequiredService<IReadOnlyRestCollection<Hierarchy, int>>();
            var collection3 = provider.GetRequiredService<CollectionPipo>();

            Assert.Equal(collection3, collection2);
        }

        [Fact]
        public void TestCollectionRegister()
        {
            var services = new ServiceCollection();
            new RddBuilder(services).AddRestCollection<CollectionPipo, Hierarchy, int>();
            var provider = services.BuildServiceProvider();

            var collection = provider.GetRequiredService<IRestCollection<Hierarchy, int>>();
            var collection2 = provider.GetRequiredService<IReadOnlyRestCollection<Hierarchy, int>>();
            var collection3 = provider.GetRequiredService<CollectionPipo>();

            Assert.Equal(collection, collection2);
            Assert.Equal(collection, collection3);
            Assert.Equal(collection3, collection2);
        }

        class ControllerPipo : IAppController<Hierarchy, int>
        {
            public Task<Hierarchy> CreateAsync(ICandidate<Hierarchy, int> candidate, Query<Hierarchy> query) => throw new NotImplementedException();
            public Task<IEnumerable<Hierarchy>> CreateAsync(IEnumerable<ICandidate<Hierarchy, int>> candidates, Query<Hierarchy> query) => throw new NotImplementedException();
            public Task<IEnumerable<Hierarchy>> CreateAsync(IEnumerable<Hierarchy> created) => throw new NotImplementedException();

            public Task DeleteByIdAsync(int id) => throw new NotImplementedException();
            public Task DeleteByIdsAsync(IEnumerable<int> ids) => throw new NotImplementedException();
            public Task<ISelection<Hierarchy>> GetAsync(Query<Hierarchy> query) => throw new NotImplementedException();
            public Task<Hierarchy> GetByIdAsync(int id, Query<Hierarchy> query) => throw new NotImplementedException();
            public Task<Hierarchy> UpdateByIdAsync(int id, ICandidate<Hierarchy, int> candidate, Query<Hierarchy> query) => throw new NotImplementedException();
            public Task<IEnumerable<Hierarchy>> UpdateByIdsAsync(IDictionary<int, ICandidate<Hierarchy, int>> candidatesByIds, Query<Hierarchy> query) => throw new NotImplementedException();
        }

        [Fact]
        public void TestReadOnlyControllerRegister()
        {
            var services = new ServiceCollection();
            new RddBuilder(services).AddReadOnlyAppController<ControllerPipo, Hierarchy, int>();
            var provider = services.BuildServiceProvider();

            Assert.Null(provider.GetService<IAppController<Hierarchy, int>>());

            var controller2 = provider.GetRequiredService<IReadOnlyAppController<Hierarchy, int>>();
            var controller3 = provider.GetRequiredService<ControllerPipo>();

            Assert.Equal(controller3, controller2);
        }

        [Fact]
        public void TestControllerRegister()
        {
            var services = new ServiceCollection();
            new RddBuilder(services).AddAppController<ControllerPipo, Hierarchy, int>();
            var provider = services.BuildServiceProvider();

            var controller = provider.GetRequiredService<IAppController<Hierarchy, int>>();
            var controller2 = provider.GetRequiredService<IReadOnlyAppController<Hierarchy, int>>();
            var controller3 = provider.GetRequiredService<ControllerPipo>();

            Assert.Equal(controller, controller2);
            Assert.Equal(controller, controller3);
            Assert.Equal(controller3, controller2);
        }

        class PatcherPipo : IPatcher<RandomClass>
        {
            public object InitialValue(PropertyInfo property, object patchedObject) => throw new NotImplementedException();
            public RandomClass Patch(RandomClass patchedObject, JsonObject json) => throw new NotImplementedException();
            public object PatchValue(object patchedObject, Type expectedType, IJsonElement json) => throw new NotImplementedException();
        }

        class RandomClass { }

        [Fact]
        public void TestPatcherRegister()
        {
            var services = new ServiceCollection();
            services.TryAddSingleton<IPatcherProvider, PatcherProvider>();
            services.TryAddSingleton<IReflectionHelper, ReflectionHelper>();
            new RddBuilder(services).AddPatcher<PatcherPipo, RandomClass>();
            var provider = services.BuildServiceProvider();

            var patcher = provider.GetRequiredService<IPatcher<RandomClass>>();
            var patcher2 = provider.GetRequiredService<PatcherPipo>();

            Assert.Equal(patcher, patcher2);

            var patcherProvider = provider.GetRequiredService<IPatcherProvider>();
            var patcher3 = patcherProvider.GetPatcher(typeof(RandomClass), null);

            Assert.Equal(patcher3, patcher2);
            Assert.Equal(patcher3, patcher);
        }

        [Theory]
        [InlineData(RightDefaultMode.Open, typeof(OpenRightExpressionsHelper<Hierarchy>))]
        [InlineData(RightDefaultMode.Closed, typeof(ClosedRightExpressionsHelper<Hierarchy>))]
        public void TestRights(RightDefaultMode mode, Type expectedType)
        {
            var services = new ServiceCollection();
            new RddBuilder(services).WithDefaultRights(mode);
            var provider = services.BuildServiceProvider();

            Assert.IsType(expectedType, provider.GetRequiredService<IRightExpressionsHelper<Hierarchy>>());
        }
    }
}