using System;
using System.Collections.Generic;

namespace RDD.Domain.Mocks
{
    public abstract class Hierarchy : IEntityBase<Hierarchy, int>
    {
        public string BaseProperty { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string Type { get; set; }

        public int Id { get; set; }

        public Hierarchy Clone() => this;

        public object GetId() => Id;

        public void SetId(object id) => Id = (int)id;
    }

    public class Super : Hierarchy
    {
        public string SuperProperty { get; set; }
    }

    public class InheritanceConfiguration : IInheritanceConfiguration<Hierarchy>
    {
        public Type BaseType => typeof(Hierarchy);

        public string Discriminator => "type";

        public IReadOnlyDictionary<string, Type> Mappings => new Dictionary<string, Type>
        {
            { "super", typeof(Super) }
        };
    }
}