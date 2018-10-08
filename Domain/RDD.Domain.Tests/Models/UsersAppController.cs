using Rdd.Application;
using Rdd.Application.Controllers;
using System;

namespace Rdd.Domain.Tests.Models
{
    public class UsersAppController : AppController<UsersCollection, User, Guid>
    {
        public UsersAppController(IStorageService storage, UsersCollection collection)
            : base(storage, collection) { }
    }
}
