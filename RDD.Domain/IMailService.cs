using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface IMailService
	{
		void SendMail(string from, string to, string subject, string body, bool forceSend = false);
		void SendExceptionMail(Exception E);
		void SendCriticalExceptionMail(string message);
	}
}
