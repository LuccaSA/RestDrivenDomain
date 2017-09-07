using RDD.Domain.Models;
using System;
using System.Net.Mail;

namespace RDD.Domain.Tests.Models
{
	public class User : EntityBase<int>
	{
		public override int Id { get; set; }
		public override string Name { get; set; }
		public MailAddress Mail { get; set; }
		public Uri TwitterUri { get; set; }
		public decimal Salary { get; set; }
	}
}
