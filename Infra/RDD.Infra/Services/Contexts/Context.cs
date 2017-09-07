using RDD.Domain;
using System;
using System.Collections.Generic;

namespace RDD.Infra.Services.Contexts
{
	class Context : IContext
	{
		IStorageService _storageService;
		Queue<Action> _saveChangesCallbacks;

		public Context(IStorageService storageService)
		{
			_storageService = storageService;
			_saveChangesCallbacks = new Queue<Action>();
		}

		public void AddSaveChangesCallback(Action action)
		{
			_saveChangesCallbacks.Enqueue(action);
		}

		public void SaveChanges()
		{
			try
			{
				_storageService.Commit();

				while(_saveChangesCallbacks.Count != 0)
				{
					_saveChangesCallbacks.Dequeue()();
				}
			}
			finally
			{
				_saveChangesCallbacks = new Queue<Action>();
			}
		}

		#region IDisposable Support
		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_storageService?.Dispose();
					_storageService = null;
				}

				disposedValue = true;
			}
		}

		~Context() { Dispose(false); }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}