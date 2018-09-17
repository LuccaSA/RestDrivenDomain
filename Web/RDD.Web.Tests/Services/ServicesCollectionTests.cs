using Microsoft.Extensions.DependencyInjection;
using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Mocks;
using RDD.Web.Helpers;
using RDD.Web.Serialization;
using RDD.Web.Serialization.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RDD.Web.Tests.Services
{
    public class ServicesCollectionTests
    {
        public abstract class Hierarchy2 : IEntityBase<Hierarchy2, int>
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

            services.AddRddInheritanceConfiguration<InheritanceConfiguration, Hierarchy, int>(new InheritanceConfiguration());
            services.AddRddInheritanceConfiguration<InheritanceConfiguration2, Hierarchy2, int>(new InheritanceConfiguration2());

            var provider = services.BuildServiceProvider();

            var configs = provider.GetRequiredService<IEnumerable<IInheritanceConfiguration>>();

            Assert.Equal(2, configs.ToList().Count);

            var config1 = provider.GetRequiredService<IInheritanceConfiguration<Hierarchy>>();
            var config2 = provider.GetRequiredService<IInheritanceConfiguration<Hierarchy2>>();
        }

        [Fact]
        public void TestEmptyInheritanceRegister()
        {
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var configs = provider.GetRequiredService<IEnumerable<IInheritanceConfiguration>>();

            Assert.Empty(configs);
        }

        class Principal : IPrincipal
        {
            public int Id => throw new NotImplementedException();
            public string Token { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public string Name => throw new NotImplementedException();
            public Culture Culture => throw new NotImplementedException();
            public PrincipalType Type => throw new NotImplementedException();
        }

        [Fact]
        public void TestRddSerializationRegister()
        {
            var services = new ServiceCollection();

            services.AddRDDSerialization<Principal>();
            var provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetRequiredService<ISerializerProvider>());
            Assert.NotNull(provider.GetRequiredService<IRDDSerializer>());
            Assert.NotNull(provider.GetRequiredService<IPrincipal>());
        }
    }
}
