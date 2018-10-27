using System;
using System.Threading.Tasks;
using Rdd.Domain.Helpers.Reflection;
using Rdd.Domain.Models;
using Rdd.Domain.Patchers;

namespace Rdd.Domain.Tests.Models
{
    public class HierarchiesCollection : RestCollection<Hierarchy, int>
    {
        private readonly IInstantiator<Hierarchy> _instanciator;

        public HierarchiesCollection(IRepository<Hierarchy, int> repository, IPatcher<Hierarchy> patcher, IInstantiator<Hierarchy> instanciator)
            : base(repository, patcher)
        {
            _instanciator = instanciator;
        }

        public HierarchiesCollection(IRepository<Hierarchy, int> repository, IPatcherProvider patcherProvider, IInstantiator<Hierarchy> instanciator)
            : this(repository, new ObjectPatcher<Hierarchy>(patcherProvider, new ReflectionHelper()), instanciator) { }

        public override Task<Hierarchy> InstantiateEntityAsync(ICandidate<Hierarchy, int> candidate)
        {
            return _instanciator.InstantiateAsync(candidate);
        }
    }
}
