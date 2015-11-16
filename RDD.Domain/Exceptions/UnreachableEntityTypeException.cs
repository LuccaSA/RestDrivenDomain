using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain.Exceptions
{
	public class UnreachableEntityTypeException<TEntity> : HttpLikeException
	{
		public UnreachableEntityTypeException()
			: base(HttpStatusCode.Forbidden, String.Format("Unreachable entity type {0}", typeof(TEntity).Name)) { }
	}
}
