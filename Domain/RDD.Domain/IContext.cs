using System;

namespace RDD.Domain
{
	public interface IContext : IDisposable
	{
		void SaveChanges();

		void AddSaveChangesCallback(Action action);
	}
}
