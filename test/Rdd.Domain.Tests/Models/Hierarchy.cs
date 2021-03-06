﻿using System;
using System.Collections.Generic;

namespace Rdd.Domain.Tests.Models
{
    public abstract class Hierarchy : IEntityBase<int>
    {
        public string BaseProperty { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string Type { get; }

        public long Value { get; set; }

        public int Id { get; set; }

        public Hierarchy Clone() => this;

        public object GetId() => Id;

        public void SetId(object id) => Id = (int)id;
    }

    public class Super : Hierarchy
    {
        public string SuperProperty { get; set; }
        public long Value2 { get; set; }
    }

    public class SuperSuper : Super
    {
        public string SuperSuperProperty { get; set; }
    }

    public class InheritanceConfiguration : IInheritanceConfiguration<Hierarchy>
    {
        public Type BaseType => typeof(Hierarchy);

        public string Discriminator => "type";

        public IReadOnlyDictionary<string, Type> Mappings => new Dictionary<string, Type>
        {
            { "super", typeof(Super) },
            { "supersuper", typeof(SuperSuper) }
        };
    }
}