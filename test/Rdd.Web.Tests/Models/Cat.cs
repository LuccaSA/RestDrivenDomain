using Rdd.Domain;

namespace Rdd.Web.Tests.Models
{
    public class Cat : IEntityBase<int>
    {
        public string Name { get; set; }

        public string Url { get; }

        public int Id { get; set; }

        public int Age { get; set; }

        public Cat() { }
        public object GetId() => Id;
    }
}
