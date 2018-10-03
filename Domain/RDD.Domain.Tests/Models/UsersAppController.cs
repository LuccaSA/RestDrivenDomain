using RDD.Application;
using RDD.Application.Controllers;
using System;

namespace RDD.Domain.Tests.Models
{
    public class UsersAppController : AppController<UsersCollection, User, Guid>
    {
        public UsersAppController(IStorageService storage, UsersCollection collection)
            : base(storage, collection) { }
    }
}
