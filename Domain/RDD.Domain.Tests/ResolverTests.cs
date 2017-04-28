using RDD.Domain.Tests.Models;
using RDD.Infra.Contexts;
using RDD.Infra.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RDD.Domain.Tests
{
	public class ResolverTests
	{
		[Fact]
		public void Resolver_SHOULD_NotResolveClassWithArguments_WHEN_NotRegistered()
		{
			var resolver = new DependencyInjectionResolver();

			resolver.Register<IFakeInterface, string>((arg1) => new OneArgumentConstructorClass(arg1));

			//Should throw resolver exception (parameterless constr not registered)
			Assert.Throws<ResolverException>(() =>
			{
				resolver.Resolve<IFakeInterface>();
			});
		}

		[Fact]
		public void Resolver_SHOULD_NotResolveClassWithArguments_WHEN_RegisteredWithWrongArgumentTypes()
		{
			var resolver = new DependencyInjectionResolver();

			resolver.Register<IFakeInterface, string>((arg1) => new OneArgumentConstructorClass(arg1));

			//Should throw resolver exception (good number of argument, but wrong type)
			Assert.Throws<ResolverException>(() =>
			{
				resolver.Resolve<IFakeInterface, int>(1);
			});
		}

		[Fact]
		public void Resolver_SHOULD_ResolveClassWithArguments_WHEN_ProperlyRegistered()
		{
			var resolver = new DependencyInjectionResolver();

			resolver.Register<IFakeInterface, string>((arg1) => new OneArgumentConstructorClass(arg1));

			//Should not fail
			var result = resolver.Resolve<IFakeInterface, string>("test");
			Assert.NotNull(result);
		}
	}
}
