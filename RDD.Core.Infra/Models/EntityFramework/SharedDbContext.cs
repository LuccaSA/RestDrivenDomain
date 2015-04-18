using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.EntityFramework
{
	public class SharedDbContext : DbContext
	{
		private IEnumerable<Action<DbModelBuilder>> _onPartCreatings;

		public SharedDbContext()
		{
			throw new Exception("You should provide a IEnumerable<Action<DbModelBuilder>> in the constructor");
		}
		public SharedDbContext(string connectionString, IEnumerable<Action<DbModelBuilder>> onPartCreatings)
			: base(connectionString)
		{
			_onPartCreatings = onPartCreatings;
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			foreach (var onPartCreating in _onPartCreatings)
			{
				onPartCreating(modelBuilder);
			}
		}
	}
}
