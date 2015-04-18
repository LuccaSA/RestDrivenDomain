using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public interface IPartialDbContext
	{
		void OnModelCreating(DbModelBuilder modelBuilder);
	}
}
