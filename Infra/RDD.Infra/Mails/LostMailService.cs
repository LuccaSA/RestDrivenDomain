using RDD.Domain;
using System;

namespace RDD.Infra.Mails
{
	public class LostMailService : IMailService
	{
		public void SendMail(string from, string to, string subject, string body, bool forceSend = false) { }
		public void SendExceptionMail(Exception E) { }
		public void SendCriticalExceptionMail(string message) { }
	}
}
