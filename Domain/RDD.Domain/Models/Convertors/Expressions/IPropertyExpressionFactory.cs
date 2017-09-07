using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RDD.Domain.Models.Convertors.Expressions
{
	public interface IPropertyExpressionFactory
	{
		MemberExpression GetMemberExpression(Type type, ParameterExpression seed, string field, out PropertyInfo property);
	}
}