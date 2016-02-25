using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Mails
{
	public class SmtpServerInfo
	{
		public string Server { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		public int Port { get; set; }
		public bool Ssl { get; set; }
	}
}
