using RDD.Domain;
using RDD.Domain.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Mails
{
	public class SmtpMailService : IMailService
	{
		private SmtpServerInfo _serverInfo;
		private ExceptionMailInfo _exceptionMailInfo;

		public SmtpMailService(SmtpServerInfo serverInfo, ExceptionMailInfo exceptionMailInfo)
		{
			_serverInfo = serverInfo;
			_exceptionMailInfo = exceptionMailInfo;
		}

		public void SendMail(string from, string to, string subject, string body, bool forceSend = false)
		{
			var logService = Resolver.Current().Resolve<ILogService>();

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
						smtpClient.Host = _serverInfo.Server;
						smtpClient.Port = _serverInfo.Port;
						smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
						smtpClient.EnableSsl = _serverInfo.Ssl;

						if (!String.IsNullOrEmpty(_serverInfo.Login))
						{
							smtpClient.UseDefaultCredentials = false;
							smtpClient.Credentials = new NetworkCredential(_serverInfo.Login, _serverInfo.Password);
						}

						smtpClient.Send(mail);
					};

					logService.Log(LogLevel.INFO, String.Format("Smtp message sent from {0} to {1} with subject {2}", from, to, subject));

					return;
				}

				logService.Log(LogLevel.INFO, String.Format("Smtp message not forced (then not sent) from {0} to {1} with subject {2}", from, to, subject));
			}
			catch (Exception E)
			{
				logService.Log(LogLevel.ERROR, String.Format("Smtp message send failure from {0} to {1} with subject {2}, error : {3}", from, to, subject, E.Message));
			}
		}

		public void SendExceptionMail(Exception E)
		{
			var webContext = Resolver.Current().Resolve<IWebContext>();

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

				try
				{
					var Detail = new Func<string, string, string>((k, v) => String.Format("{0} : {1}<br /><br />", k, v));

					body += Detail("Page REST", webContext.RawUrl);
					body += Detail("Page", webContext.Url.ToString());
					body += Detail("Host Address", webContext.UserHostAddress);
					body += Detail("Host Name", webContext.UserHostName);
					body += Detail("User Agent", webContext.UserAgent);
				}
				catch { } // No context

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
