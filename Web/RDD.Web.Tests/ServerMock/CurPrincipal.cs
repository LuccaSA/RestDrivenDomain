using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using RDD.Domain;
using RDD.Domain.Helpers;

namespace RDD.Web.Tests.ServerMock
{
    public class CurPrincipal : IPrincipal
    {
        public int Id { get; }
        public string Token { get; set; }
        public string Name { get; }
        public Culture Culture { get; }

        public PrincipalType Type => PrincipalType.User;
    }
}