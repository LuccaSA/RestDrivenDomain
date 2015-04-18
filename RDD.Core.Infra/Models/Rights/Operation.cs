using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Rights
{
	public class Operation
	{
		private Func<string> _culturedName;

		public int Id { get; private set; }
		public string Code { get; private set; }
		public string Name { get { return _culturedName(); } }

		public Operation(int id, string code)
		{
			Id = id;
			Code = code;
			_culturedName = () => code;
		}
		public Operation(int id, string code, string name)
		{
			Id = id;
			Code = code;
			_culturedName = () => name;
		}
		public Operation(int id, string code, Func<string> culturedName)
		{
			Id = id;
			Code = code;
			_culturedName = culturedName;
		}
	}
}
