using RDD.Application.Controllers;
using RDD.Infra;

namespace RDD.Domain.Tests.Models
{
    public class UsersAppController : AppController<UsersCollection, User, int>
    {
        public UsersAppController(IStorageService storage, UsersCollection collection)
            : base(storage, collection) { }
    }
}
