namespace RDD.Web.Tests.Models
{
    public class Owner
    {
        public string Name { get; set; }
        public Owner Manager1 { get; set; }
        public Owner Manager2 { get; set; }
    }
}