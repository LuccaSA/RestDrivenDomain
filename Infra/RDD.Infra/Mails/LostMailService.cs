using RDD.Domain;
using System;

namespace RDD.Infra.Mails
{
	public class LostMailService : IMailService
	{
		public void SendMail(string from, string to, string subject, string body, bool forceSend = false)
		{
			//Silently does not send mail
		}
		public void SendExceptionMail(Exception e)
		{
			//Silently does not send exception mail
		}
		public void SendCriticalExceptionMail(string message)
		{
			//Silently does not send critical exception mail
		}
	}
}
