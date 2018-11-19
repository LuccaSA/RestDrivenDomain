namespace Rdd.Domain.Tests.Models
{
    public class OptionalChild
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public Parent Parent { get; set; }

        public string Name { get; set; }
    }
}
