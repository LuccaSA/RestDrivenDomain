using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Models
{
	public class Operation
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public Func<string> CultureLabel { get; set; }
	}
}
