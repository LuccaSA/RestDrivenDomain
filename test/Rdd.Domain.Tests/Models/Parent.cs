namespace Rdd.Domain.Tests.Models
{
    public class Parent : IEntityBase<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public OptionalChild OptionalChild { get; set; }

        public string Url { get; }

        public object GetId() => Id;
    }
}
