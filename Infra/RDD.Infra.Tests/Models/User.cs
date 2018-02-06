using RDD.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RDD.Infra.Tests.Models
{
    public class User : EntityBase<User, int>
    {
        public override int Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
