namespace RDD.Infra.Mails
{
	public interface ISmtpServer
	{
		string Login { get; }
		string Password { get; }
		int Port { get; }
		string Server { get; }
		bool Ssl { get; }
	}
}