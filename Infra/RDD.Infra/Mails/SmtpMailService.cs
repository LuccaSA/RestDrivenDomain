using RDD.Domain;
using System;
using System.Net;
using System.Net.Mail;

namespace RDD.Infra.Mails
{
	public class SmtpMailService : IMailService
	{
		private ISmtpServer _smtpServer;
		private ExceptionMail _exceptionMailInfo;

		public SmtpMailService(ISmtpServer smtpServer, ExceptionMail exceptionMailInfo)
		{
			_smtpServer = smtpServer;
			_exceptionMailInfo = exceptionMailInfo;
		}

		public void SendMail(string from, string to, string subject, string body, bool forceSend = false)
		{
			try
			{
				if (forceSend)
				{
					var mail = new MailMessage
					{
						From = new MailAddress(from),
						Subject = subject,
						Body = body,
						IsBodyHtml = true
					};

					mail.To.Add(to);

					using (var smtpClient = new SmtpClient())
					{
						smtpClient.Host = _smtpServer.Server;
						smtpClient.Port = _smtpServer.Port;
						smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
						smtpClient.EnableSsl = _smtpServer.Ssl;

						if (!String.IsNullOrEmpty(_smtpServer.Login))
						{
							smtpClient.UseDefaultCredentials = false;
							smtpClient.Credentials = new NetworkCredential(_smtpServer.Login, _smtpServer.Password);
						}

						smtpClient.Send(mail);
					};

					return;
				}
			}
			catch { }
		}

		public void SendExceptionMail(Exception E)
		{
			try
			{
				var stackTrace = String.Empty;

				//Si l'Exception est instanciée à la mano, y'a pas de StackTrace
				if (E.StackTrace != null)
				{
					stackTrace = E.StackTrace.Replace("\r\n", "<br />");
				}

				var body = String.Format("Erreur : {0}<br /><br />{1}<br /><br />", E.Message, stackTrace);

				if (E.InnerException != null)
				{
					var innerStackTrace = String.Empty;

					if (E.InnerException.StackTrace != null)
					{
						innerStackTrace = E.InnerException.StackTrace.Replace("\r\n", "<br />");
					}

					body += String.Format("Inner : {0}<br /><br />{1}<br /><br />", E.InnerException.Message, innerStackTrace);
				}

				SendMail(_exceptionMailInfo.Sender, _exceptionMailInfo.Recipient, _exceptionMailInfo.Subject, body, true);
			}
			catch { }
		}

		public void SendCriticalExceptionMail(string message)
		{
			var body = String.Format("Critical log level reached : {1}", message);

			SendMail(_exceptionMailInfo.Sender, _exceptionMailInfo.Recipient, _exceptionMailInfo.Subject, body, true);
		}
	}
}
