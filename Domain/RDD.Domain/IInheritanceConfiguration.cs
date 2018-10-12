using System;
using System.Collections.Generic;

namespace Rdd.Domain
{
    public interface IInheritanceConfiguration
    {
        Type BaseType { get; }
        string Discriminator { get; }
        IReadOnlyDictionary<string, Type> Mappings { get; }
    }

    public interface IInheritanceConfiguration<TEntityBase> : IInheritanceConfiguration
    {
    }
}
