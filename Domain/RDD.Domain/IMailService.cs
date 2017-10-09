using System;

namespace RDD.Domain
{
	public interface IMailService
	{
		void SendMail(string from, string to, string subject, string body, bool forceSend = false);
		void SendExceptionMail(Exception e);
		void SendCriticalExceptionMail(string message);
	}
}
