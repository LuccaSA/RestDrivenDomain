using System;
using System.Collections;
using System.Linq.Expressions;

namespace RDD.Domain.Models.Convertors.Expressions
{
	public interface IExpressionGenerator<T> where T : class
	{
		Expression<Func<T, bool>> Equals(string field, IList values);
		Expression<Func<T, bool>> NotEqual(string field, IList values);
		Expression<Func<T, bool>> Starts(string field, IList values);
		Expression<Func<T, bool>> Like(string field, IList values);

		Expression<Func<T, bool>> Between(string field, IList values);
		Expression<Func<T, bool>> Until(string field, IList values);
		Expression<Func<T, bool>> Since(string field, IList values);

		Expression<Func<T, bool>> GreaterThan(string field, IList values);
		Expression<Func<T, bool>> GreaterThanOrEqual(string field, IList values);
		Expression<Func<T, bool>> LessThan(string field, IList values);
		Expression<Func<T, bool>> LessThanOrEqual(string field, IList values);
	}
}
