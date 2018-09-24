using RDD.Domain;
using RDD.Domain.Models;
using System;
using System.Collections.Generic;

namespace RDD.Web.Tests.Models
{
    public class User : EntityBase<User, int>, IUser
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
        public MyValueObject MyValueObject { get; set; }
        public Uri TwitterUri { get; set; }
        public decimal Salary { get; set; }
        public Department Department { get; set; }
        public Guid PictureId { get; set; }
        public DateTime? BirthDay { get; set; }
        public DateTime ContractStart { get; set; }
        public List<UserFile> Files { get; set; }

        IUser ICloneable<IUser>.Clone()
        {
            return this;
        }
    }

    public class UserFile
    {
        public Guid Id { get; set; }
        public DateTime DateUpload { get; set; }
        public FileDescriptor FileDescriptor { get; set; }
    }

    public class FileDescriptor
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
    }
}
